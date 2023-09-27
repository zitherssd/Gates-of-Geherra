using Assets.Scripts.Actions;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets
{
    public class BaseActorBattler : MonoBehaviour
    {
        [SerializeField]
        public Actor Actor;
        private MoveState state;
        private GameObject selectionCircle;
        private Vector3 slideTargetPosition;
        private Action onMoveComplete;
        private Action onAnimationHitComplete;
        private Animator animator;
        private Rigidbody rigidbody;
        private float lastSqrMag;
        private bool cancelMoveCallback;
        private AudioSource audioSource;
        internal bool isBlocking;
        internal bool collisionOccured;
        public GameObject damagePopupPrefab;
        public GameObject posturePopupPrefab;

        public enum MoveState
        {
            Idle,
            Knockback,
            Sliding,
            Move,
            Busy,
            Slerp
        }

        private void Awake()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            rigidbody = gameObject.GetComponent<Rigidbody>();
            animator = gameObject.GetComponent<Animator>();
            selectionCircle = GameObject.FindGameObjectWithTag("SelectionCircle");
            state = MoveState.Idle;

            //var cc = GetComponentInChildren<ColorController>();
            //cc.mainColor = baseActor.mainColor;
            //cc.secondaryColor = baseActor.secondaryColor;
        }

        private void Update()
        {
            switch (state)
            {
                case MoveState.Idle:
                    break;
                case MoveState.Sliding:

                    if (rigidbody.velocity.magnitude < 0.01f)
                    {
                        state = MoveState.Idle;
                        onMoveComplete();
                    }

                    break;
                case MoveState.Busy:
                    break;
                case MoveState.Move:
                    var sqrMag = (slideTargetPosition - transform.position).sqrMagnitude;
                    float moveSpeed = 4f;
                    rigidbody.velocity = (slideTargetPosition - transform.position).normalized * moveSpeed;

                    if (sqrMag > lastSqrMag)
                    {
                        rigidbody.velocity = Vector3.zero;
                        state = MoveState.Idle;
                        if (!cancelMoveCallback)
                            onMoveComplete();
                        else return;
                    }

                    lastSqrMag = sqrMag;
                    break;
                case MoveState.Knockback:
                    break;
                case MoveState.Slerp:
                    sqrMag = (slideTargetPosition - transform.position).sqrMagnitude;
                    moveSpeed = 4f;
                    rigidbody.transform.position = (Vector3.Slerp(rigidbody.transform.position, slideTargetPosition, 0.01f));

                    if (sqrMag > lastSqrMag)
                    {
                        rigidbody.velocity = Vector3.zero;
                        state = MoveState.Idle;
                        if (!cancelMoveCallback)
                            onMoveComplete();
                        else return;
                    }

                    lastSqrMag = sqrMag;
                    break;
            }
        }

        public void Initialize()
        {
            var actor = GetBaseActor();
            //actor.OnDamageDealt += ShowDamagePopup;
            //actor.OnPostureDamage += ShowPosturePopup;
        }

        public void UseSkill(BaseSkill skill, Action onSkillComplete)
        {
            if (skill.TotalUses != 0)
                skill.remainingUses -= 1;

            UIManager.GetInstance().SetTextThenFade($"{this.GetBaseActor().Name} uses {skill.Name}!", 0.5f);

            if (skill.TargetOption == TARGETINGOPTION.ENEMY && isControllable())
            {
                InputManager.instance.WaitForTargetActor(targetActor => PerformSkillOnTarget(skill, targetActor, onSkillComplete));
                return;
            }

            if (skill.TargetOption == TARGETINGOPTION.ENEMY)
            {
                //Enemy attacking me
                var targetActor = BattleManager.GetInstance().PlayerActors[0];
                UIManager.GetInstance().DrawAllActorReactionsAboveSpeed(targetActor, skill.Speed, () => PerformSkillOnTarget(skill, targetActor, onSkillComplete));
                return;
            }

            if (skill.TargetOption == TARGETINGOPTION.SELF)
            {
                PerformSkillOnSelf(skill, onSkillComplete);
            }
        }

        private void PerformSkillOnSelf(BaseSkill skill, Action onSkillComplete)
        {
            skill.WarmUp(this, null, () =>
            {
                UIManager.GetInstance().AddToStoneSlab($"{this.GetBaseActor().Name} uses {skill.Name}");
                skill.PerformSelf(this, onSkillComplete);
            });
        }

        private void PerformSkillOnTarget(BaseSkill skill, BaseActorBattler targetActor, Action onSkillComplete)
        {

            BaseReaction targetReaction;
            if (skill.Tags.Contains(SKILLTAG.NO_REACTION))
            {
                targetReaction = BaseReaction.NoReaction;
            }
            else
            {
                if(targetActor.isControllable())
                {
                    targetReaction = UIManager.GetInstance().GetSelectedReaction();
                }
                else
                {
                    targetReaction = targetActor.ChooseValidReactionAtRandom(skill.Speed);
                }
            }

            skill.WarmUp(this, targetActor, () =>
            {
                targetReaction.WarmUp(targetActor, () =>
                {
                    skill.PerformTarget(this, targetActor, targetReaction, () =>
                    {
                        targetReaction.React(targetActor, () =>
                        {
                            skill.ApplyDamageEffects(this, targetActor, targetReaction, onSkillComplete);
                        });
                    });
                });
            });
        }

        public void ApplyKnockback(Vector3 direction, float force, Action onKnockbackFinished)
        {
            var animator = this.GetComponent<Animator>();

            if (force > 0)
            {
                //Animation
                if (!isBlocking)
                    if (direction.y > 0.1)
                        animator.Play("HurtAir");
                    else
                        animator.Play("HurtGround");

                //Force
                this.MoveToPosition(transform.position + direction.normalized * force, MoveState.Sliding, () => { animator.Play("Idle"); onKnockbackFinished(); });
                //Blood particles
                //GetComponentInChildren<ParticleSystem>().Play();

            }
            else
                onKnockbackFinished();

        }

        public void MoveToPosition(Vector3 TargetPosition, MoveState state, Action onMoveComplete)
        {
            if (state == MoveState.Sliding)
                rigidbody.velocity = (TargetPosition - transform.position);

            lastSqrMag = Mathf.Infinity;
            this.slideTargetPosition = TargetPosition;
            this.onMoveComplete = onMoveComplete;
            this.state = state;
        }

        public void Move(Vector3 direction, Action onMoveComplete)
        {
            UIManager.GetInstance().AddToStoneSlab($"{Actor.Name} moves");
            PlayAudio("Move2");

            lastSqrMag = Mathf.Infinity;
            this.slideTargetPosition = transform.position + (direction * Actor.AGI);
            this.onMoveComplete = onMoveComplete;
            this.state = MoveState.Move;

        }

        public void MoveRelativeToCamera(Vector2 direction, Action onMoveComplete)
        {
            var camera = Camera.main;
            var forward = camera.transform.forward; forward.y = 0;
            var right = camera.transform.right; right.y = 0;
            forward.Normalize(); right.Normalize();

            var desiredMoveDirection = forward * direction.y + right * direction.x;
            Move(desiredMoveDirection, onMoveComplete);
        }

        public void PlayAnimation(string AnimationName, Action onAnimationHitComplete)
        {
            animator.Play(AnimationName);
            this.onAnimationHitComplete = onAnimationHitComplete;
        }
        public void PlayAnimation(string AnimationName)
        {
            animator.Play(AnimationName);
        }

        public BaseReaction ChooseValidReactionAtRandom(int minimumReactionSpeed)
        {
            List<BaseReaction> validreactions = new List<BaseReaction>();

            validreactions = Actor.reactions.Where(reaction => reaction.Speed >= minimumReactionSpeed && reaction.HasUsesLeft()).ToList();
            if (validreactions.Count == 0) return BaseReaction.NoReaction;

            var random = new System.Random();
            var index = random.Next(validreactions.Count);

            return validreactions[index];
        }

        public BaseSkill ChooseValidSkillAtRandom()
        {
            List<BaseSkill> validskills = new List<BaseSkill>();

            validskills = Actor.skills.Where(skills => skills.IsValid(this)).ToList();

            if (validskills.Count == 0) return new BaseSkill { TargetOption = TARGETINGOPTION.SELF, Name = "Nothing" };

            var random = new System.Random();
            var index = random.Next(validskills.Count);

            return validskills[index];
        }

        public BaseSkill ChooseValidSkillAtRandomOrReturnNull()
        {
            List<BaseSkill> validskills = new List<BaseSkill>();

            validskills = Actor.skills.Where(skills => skills.IsValid(this)).ToList();

            if (validskills.Count == 0) return null;

            var random = new System.Random();
            var index = random.Next(validskills.Count);

            return validskills[index];
        }

        public void PlayAudio(string clipName)
        {
            var clip = SoundManager.instance.GetAudioClipByName(clipName);
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void ShowSelectionCircle()
        {
            selectionCircle.GetComponent<SelectionCircle>().target = this.transform;
        }

        public void AnimationHit()
        {
            onAnimationHitComplete();
        }

        public void ResetFlags()
        {
            isBlocking = false;
            collisionOccured = false;
        }

        public bool isControllable()
        {
            return Actor.Controllable;
        }

        public Actor GetBaseActor()
        {
            return Actor;
        }

        public Actor SetBaseActor(Actor actor)
        {
            Actor = actor;
            return Actor;
        }

        private void OnCollisionEnter(Collision collision)
        {
            var velocityThreshold = 1;

            // Check if collided object has the "Level" tag
            if (collisionOccured) return;

            if (collision.gameObject.CompareTag("Level"))
            {
                Debug.Log($"Velocity on collision is {rigidbody.velocity.magnitude / Time.deltaTime}");

                UIManager.GetInstance().AddToStoneSlab($"{Actor.Name} hits a wall!");
                Actor.DealPostureDamage(5f);
                collisionOccured = true;

                // Check if velocity magnitude is greater than the threshold
                if (rigidbody.velocity.magnitude / Time.deltaTime > 0.00001)
                {
                    // Calculate mirrored velocity (mirror along current velocity)
                    Vector3 mirroredVelocity = Vector3.Reflect(rigidbody.velocity, collision.contacts[0].normal);

                    // Replace current velocity with the mirrored velocity
                    rigidbody.velocity = mirroredVelocity;
                }
            }
        }

        public void TriggerHitstop(float duration)
        {
            Time.timeScale = 0.0f; // Pause the game
            StartCoroutine(ResumeTimeScale(duration));
        }

        // Coroutine to resume normal time scale after a duration
        private IEnumerator ResumeTimeScale(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f; // Restore the original time scale
        }

       

        public void HandlePostureBreak(Actor actor)
        {
            TriggerHitstop(0.5f);
            PlayAnimation("PostureBroken");
            PlayAudio("Attack1");
            UIManager.GetInstance().AddToStoneSlab("Posture break!");
            BattleManager.GetInstance().RepeatTurn(); //Posture break check
        }

        public void HandleOnDeath(Actor actor)
        {
            //PlayAnimation("Down");
        }

        public void ShowDamagePopup(float damageAmount)
        {
            // Instantiate the damage popup prefab
            GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);

            popup.GetComponentInChildren<DamagePopup>().Initialize(damageAmount);

        }

        public void ShowPosturePopup(float damageAmount)
        {
            // Instantiate the damage popup prefab
            GameObject popup = Instantiate(posturePopupPrefab, transform.position, Quaternion.identity);

            // Set the damage amount text
            popup.GetComponentInChildren<DamagePopup>().Initialize(damageAmount);

        }

        private void OnDestroy()
        {
            GetBaseActor().OnDamageDealt -= ShowDamagePopup;
            GetBaseActor().OnPostureDamage -= ShowPosturePopup;
        }
    }
}
