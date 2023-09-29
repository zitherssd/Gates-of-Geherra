using Assets.Scripts.Actions;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public class BaseActorBattler : MonoBehaviour
    {
        [SerializeField]
        public Actor Actor;
        public MoveState currentState;
        private GameObject selectionCircle;
        private Vector3 slideTargetPosition;
        private Action onMoveComplete;
        private Action onAnimationHitComplete;
        private Animator animator;
        public Rigidbody rigidbody;
        private float lastSqrMag;
        private bool cancelMoveCallback;
        private AudioSource audioSource;
        internal bool isBlocking;
        internal bool collisionOccured;
        public GameObject damagePopupPrefab;
        public GameObject posturePopupPrefab;
        public LayerMask layer;



        private void Awake()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            rigidbody = gameObject.GetComponent<Rigidbody>();
            animator = gameObject.GetComponent<Animator>();
            selectionCircle = GameObject.FindGameObjectWithTag("SelectionCircle");
            currentState = MoveState.Idle;

            //var cc = GetComponentInChildren<ColorController>();
            //cc.mainColor = baseActor.mainColor;
            //cc.secondaryColor = baseActor.secondaryColor;
        }
        private void Update()
        {
            switch (currentState)
            {
                case MoveState.Idle:
                    break;
                case MoveState.Sliding:

                    if (rigidbody.velocity.magnitude < 0.01f)
                    {
                        currentState = MoveState.Idle;
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
                        currentState = MoveState.Idle;
                        if (!cancelMoveCallback)
                            onMoveComplete();
                        else return;
                    }

                    lastSqrMag = sqrMag;
                    break;
                case MoveState.Knockback:
                    if (rigidbody.velocity.magnitude < 0 + float.Epsilon)
                        currentState = MoveState.Idle;
                    break;
                case MoveState.Slerp:
                    sqrMag = (slideTargetPosition - transform.position).sqrMagnitude;
                    moveSpeed = 4f;
                    rigidbody.transform.position = (Vector3.Slerp(rigidbody.transform.position, slideTargetPosition, 0.01f));

                    if (sqrMag > lastSqrMag)
                    {
                        rigidbody.velocity = Vector3.zero;
                        currentState = MoveState.Idle;
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



        public void StartCombo(Action onComboEnd)
        {
            //Pre-emptive check
            List<BaseSkill> validSkills = GetValidStartComboSkills();
            if (validSkills.Count == 0)
            {
                onComboEnd();
                return;
            }

            //Flow for player
            if (isControllable())
            {
                UIManager.GetInstance().DrawSkillsAndWaitForSelectionOrNull(validSkills, selectedSkill =>
                {
                    if (selectedSkill == null) onComboEnd();
                    else
                    {
                        if (selectedSkill.Tags.Contains(SKILLTAG.FINISHER)) UseSkill(selectedSkill, onComboEnd);
                        else UseSkill(selectedSkill, () => ContinueCombo(onComboEnd));
                    }
                });
                //Draw all valid skills and Wait for selecting a skill.
                //If Skill has been selected > Use that skill > OnSkillComplete call continue Combo
                //if SKill is null call onComboEnd
                return;
            }

            //Flow for enemy
            var selectedSkill = validSkills[RandomFromList(validSkills.Count)];
            if (selectedSkill.Tags.Contains(SKILLTAG.FINISHER)) UseSkill(selectedSkill, onComboEnd);
            else UseSkill(selectedSkill, () => ContinueCombo(onComboEnd));
        }
        public void StartCombo(BaseSkill skill, Action onComboEnd)
        {
            if (skill.Tags.Contains(SKILLTAG.FINISHER)) UseSkill(skill, onComboEnd);
            else UseSkill(skill, () => ContinueCombo(onComboEnd));
        }
        public void ContinueCombo(Action onComboEnd)
        {
            //If enemy is dead return to onComboEnd
            if (BattleManager.GetInstance().TestBattleOver())
            {
                onComboEnd();
                return;
            }

            //Pre-emptive check
            List<BaseSkill> validSkills = GetValidContinueComboSkills();
            if (validSkills.Count == 0)
            {
                onComboEnd();
                return;
            }

            //Flow for Player
            if (isControllable())
            {
                Time.timeScale = 0f;
                UIManager.GetInstance().DrawSkillsAndWaitForSelectionOrNull(validSkills, selectedSkill =>
                {
                    if (selectedSkill == null)
                    {
                        onComboEnd();
                        Time.timeScale = 1f;
                    }
                    else
                    {
                        if (selectedSkill.Tags.Contains(SKILLTAG.FINISHER)) UseSkill(selectedSkill, onComboEnd);
                        else UseSkill(selectedSkill, () => ContinueCombo(onComboEnd));
                        Time.timeScale = 1f;
                    }
                });
                return;
            }

            //Flow for Enemy
            var selectedSkill = validSkills[RandomFromList(validSkills.Count)];
            if (selectedSkill.Tags.Contains(SKILLTAG.FINISHER)) UseSkill(selectedSkill, onComboEnd);
            else UseSkill(selectedSkill, () => ContinueCombo(onComboEnd));
        }
        public void UseSkill(BaseSkill skill, Action onSkillComplete)
        {
            skill.UpdateReaminingUses();
            skill.ResetCooldown();

            UIManager.GetInstance().SetTextThenFade($"{this.GetBaseActor().Name} uses {skill.Name}!", 0.5f);

            if (isControllable())
            {
                if (skill.TargetOption == TARGETINGOPTION.ENEMY)
                    InputManager.instance.WaitForTargetActor(targetActor => PerformSkillOnTarget(skill, targetActor, onSkillComplete));
                if (skill.TargetOption == TARGETINGOPTION.SELF)
                    PerformSkillOnSelf(skill, onSkillComplete);
                return;
            }

            if (skill.TargetOption == TARGETINGOPTION.ENEMY)
            {
                //Enemy attacking me
                var targetActor = BattleManager.GetInstance().PlayerActors[0];
                PerformSkillOnTarget(skill, targetActor, onSkillComplete);
                return;
            }

            if (skill.TargetOption == TARGETINGOPTION.SELF)
            {
                PerformSkillOnSelf(skill, onSkillComplete);
            }
        }



        public List<BaseSkill> GetValidContinueComboSkills()
        {
            return Actor.skills.Where(skill => skill.IsValid(this) && !skill.Tags.Contains(SKILLTAG.STARTER)).ToList();
        }
        public List<BaseSkill> GetValidStartComboSkills()
        {
            return Actor.skills.Where(skill => skill.IsValid(this)).ToList();
        }
        public List<BaseReaction> GetValidReactionsForSkill(BaseSkill skill)
        {
            return Actor.reactions.Where(reaction => reaction.IsValid(this)).ToList();
        }



        public void ApplyKnockback(Vector3 direction, float force)
        {
            var animator = this.GetComponent<Animator>();
            if (force > 0)
            {
                //Animation
                if (!isBlocking)
                {
                    if (direction.y > 0.1)
                        animator.Play("HurtAir");
                    else
                        animator.Play("HurtGround");
                    currentState = MoveState.Knockback;
                }

                //Force
                rigidbody.AddForce(direction * 100 * force);

                //this.MoveToPosition(transform.position + direction.normalized * force, MoveState.Sliding, () => { animator.Play("Idle"); onKnockbackFinished(); });
                //Blood particles
                //GetComponentInChildren<ParticleSystem>().Play();
            }
        }
        public void MoveToPosition(Vector3 TargetPosition, MoveState state, Action onMoveComplete)
        {
            if (state == MoveState.Sliding)
                rigidbody.velocity = (TargetPosition - transform.position);

            lastSqrMag = Mathf.Infinity;
            this.slideTargetPosition = TargetPosition;
            this.onMoveComplete = onMoveComplete;
            this.currentState = state;
        }
        public void Move(Vector3 direction, Action onMoveComplete)
        {
            UIManager.GetInstance().AddToStoneSlab($"{Actor.Name} moves");
            PlayAudio("Move2");

            lastSqrMag = Mathf.Infinity;
            this.slideTargetPosition = transform.position + (direction * Actor.AGI);
            this.onMoveComplete = onMoveComplete;
            this.currentState = MoveState.Move;

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
            animator.Play(AnimationName, -1, 0);
            this.onAnimationHitComplete = onAnimationHitComplete;
        }
        public void PlayAnimation(string AnimationName)
        {
            animator.Play(AnimationName, -1, 0);
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
            //No reaction tag check
            if (skill.Tags.Contains(SKILLTAG.NO_REACTION))
            {
                PerformSkillFlow(skill, targetActor, BaseReaction.NoReaction, onSkillComplete);
            }
            else
            {
                //Get all valid reactions
                var validReactions = targetActor.GetValidReactionsForSkill(skill);
                if(targetActor.isControllable())
                {
                    //Player handling
                    Time.timeScale = 0f;
                    UIManager.GetInstance().DrawReactionsAndWaitForSelectionOrNull(validReactions, selectedReaction =>
                    {
                        Time.timeScale = 1f;
                        PerformSkillFlow(skill, targetActor, selectedReaction, onSkillComplete);
                    });
                    return;
                }
                else
                {
                    //Enemy handling
                    PerformSkillFlow(skill, targetActor, validReactions[RandomFromList(validReactions.Count)], onSkillComplete);
                }
            }
        }
        private void PerformSkillFlow(BaseSkill skill, BaseActorBattler targetActor, BaseReaction targetReaction, Action onSkillComplete)
        {
            skill.WarmUp(this, targetActor, () =>
            {
                targetReaction.WarmUp(targetActor, () =>
                {
                    skill.PerformTarget(this, targetActor, targetReaction, () =>
                    {
                        targetReaction.React(targetActor, () =>
                        {
                            // Range check
                            if (Vector3.Distance(this.transform.position, targetActor.transform.position) <= skill.Range)
                                skill.ApplyDamageEffects(this, targetActor, targetReaction, onSkillComplete);
                            else
                                onSkillComplete();
                        });
                    });
                });
            });
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
            foreach(var skill in Actor.skills)
            {
                skill.UpdateCooldown();
            }
            foreach(var reaction in Actor.reactions)
            {
                reaction.UpdateCooldown();
            }
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
                if (Actor.DealPostureDamage(5f)) HandlePostureBreak();
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
        private IEnumerator ResumeTimeScale(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f; // Restore the original time scale
        }
        public void HandlePostureBreak()
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
        private int RandomFromList(int count)
        {
            var random = new System.Random();
            var index = random.Next(count);
            return index;
        }
        private void OnDestroy()
        {
            GetBaseActor().OnDamageDealt -= ShowDamagePopup;
            GetBaseActor().OnPostureDamage -= ShowPosturePopup;
        }
    }

    public enum MoveState
    {
        Idle,
        Knockback,
        Sliding,
        Move,
        Busy,
        Slerp
    }

}
