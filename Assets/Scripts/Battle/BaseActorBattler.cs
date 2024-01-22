using Assets.Scripts.Actions;
using Assets.Scripts.Battle.States;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
        private Action onReactionCheck;
        private Action onAnimationComplete;
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
        public List<BaseStatus> activeStates;
        public UnityAction<float> onDamageRecieved;
        public UnityAction<float> onPostureRecieved;
        public UnityAction<Vector3, float> onKnockbackRecieved;



        private void Awake()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            rigidbody = gameObject.GetComponent<Rigidbody>();
            animator = gameObject.GetComponent<Animator>();
            selectionCircle = GameObject.FindGameObjectWithTag("SelectionCircle");
            currentState = MoveState.Idle;
            activeStates = new List<BaseStatus>();

            onDamageRecieved += DoDamageRecievedEffects;
            onPostureRecieved += DoPostureRecievedEffects;
            onKnockbackRecieved += DoKnockbackRecievedEffects;
            //var cc = GetComponentInChildren<ColorController>();
            //cc.mainColor = baseActor.mainColor;
            //cc.secondaryColor = baseActor.secondaryColor;
        }
        private void Update()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                if (onAnimationComplete != null)
                {
                    onAnimationComplete();
                    onAnimationComplete = null;
                }
            }
        }
        private void FixedUpdate()
        {
            switch (currentState)
            {
                case MoveState.Idle:
                    break;
                case MoveState.Sliding:
                    if (rigidbody.velocity.magnitude < Mathf.Epsilon) ;
                    {
                        if (onMoveComplete != null)
                        {
                            currentState = MoveState.Idle;
                            onMoveComplete();
                            onMoveComplete = null;
                        }
                    }
                    break;
                case MoveState.Busy:
                    break;
                case MoveState.Move:
                    var sqrMag = (slideTargetPosition - transform.position).sqrMagnitude;
                    float moveSpeed = 4f;
                    if (sqrMag > lastSqrMag)
                    {
                        if (onMoveComplete != null)
                        {
                            currentState = MoveState.Idle;
                            onMoveComplete();
                            onMoveComplete = null;
                        }
                    }
                    else
                    {
                        rigidbody.velocity = (slideTargetPosition - transform.position).normalized * moveSpeed;
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


        internal void Act(Action onActorActionFinished)
        {
            var bm = BattleManager.GetInstance();
            var im = InputManager.instance;
            var um = UIManager.GetInstance();

            UIManager.GetInstance().ChangeStatus(Actor.Name + " TO ACT");
            if (isControllable())
            {
                var AvaliableSkills = Actor.skills;
                Time.timeScale = 0f;
                um.DrawActionsAndWaitForSelectionOrNull(AvaliableSkills, selectedSkill =>
                {
                    if (selectedSkill == null)
                    {
                        Time.timeScale = 1f;
                        onActorActionFinished();
                    }
                    else
                    {
                        if (selectedSkill.Tags.Contains(TAG.USESLIDER))
                            selectedSkill.SliderValue = InputManager.instance.SliderValue;
                        if (selectedSkill.Tags.Contains(TAG.USEKNOB))
                            selectedSkill.StickValue = InputManager.instance.StickValue;

                        Time.timeScale = 1f;
                        UseAction(selectedSkill, onActorActionFinished);
                    }
                });
            }
            else
            {
                //Enemy AI here basically
                var chosenSkill = ChooseValidSkillInRangeOrReturnNull();
                if (chosenSkill == null)
                {
                    Move((bm.PlayerActors[0].transform.position - transform.position).normalized, onActorActionFinished);
                }
                else
                {
                    chosenSkill.SliderValue = 1f;
                    chosenSkill.StickValue = Vector2.zero;
                    UseAction(chosenSkill, onActorActionFinished);
                }

            }
        }

        public void UseAction(BaseAction action, Action onSkillComplete)
        {
            UIManager.GetInstance().SetTextThenFade($"{this.GetBaseActor().Name} uses {action.Name}!", 0.5f);
            action.UpdateReaminingUses();
            action.ResetCooldown();
            action.Perform(this, onSkillComplete);
            var bm = BattleManager.GetInstance();

            //Hack
            bm.PlayerActors[0].transform.rotation = Quaternion.LookRotation(bm.EnemyActors[0].transform.position - bm.PlayerActors[0].transform.position, Vector3.up);
            bm.EnemyActors[0].transform.rotation = Quaternion.LookRotation(bm.PlayerActors[0].transform.position - bm.EnemyActors[0].transform.position, Vector3.up);
        }



        public List<BaseAction> GetValidContinueComboSkills()
        {
            return Actor.skills.Where(skill => skill.IsValid() && !skill.Tags.Contains(TAG.STARTER)).ToList();
        }
        public List<BaseAction> GetValidStartComboSkills()
        {
            return Actor.skills.Where(skill => skill.IsValid()).ToList();
        }
        public List<BaseReaction> GetValidReactionsForSkill()
        {
            return Actor.reactions.Where(reaction => reaction.IsValid()).ToList();
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

            this.slideTargetPosition = transform.position + (direction * Actor.AGI);
            this.onMoveComplete = onMoveComplete;
            this.currentState = MoveState.Move;
            lastSqrMag = Mathf.Infinity;
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
        public void PlayAnimation(string AnimationName, Action onAnimationHitComplete, Action onReactionCheck)
        {
            animator.Play(AnimationName, -1, 0);
            this.onReactionCheck = onReactionCheck;
            this.onAnimationHitComplete = onAnimationHitComplete;
        }
        public void PlayAnimation(string AnimationName, Action onAnimationHitComplete )
        {
            animator.Play(AnimationName, -1, 0);
            this.onAnimationHitComplete = onAnimationHitComplete;
        }

        public void SetRootMotion(bool applyRootMotion)
        {
            animator.applyRootMotion = applyRootMotion;
        }

        public void PlayAnimation(string AnimationName)
        {
            animator.Play(AnimationName, -1, 0);
        }


        private void PerformSkillFlow(BaseAction skill, BaseActorBattler targetActor, BaseReaction targetReaction, Action onSkillComplete)
        {
            //skill.WarmUp(this, targetActor, () =>
            //{
            //    skill.PerformTarget(this, targetActor, targetReaction, () =>
            //    {
            //        WaitForAnimation(onSkillComplete);
            //    });
            //});
        }




        public BaseAction ChooseValidSkillInRangeOrReturnNull()
        {
            List<BaseAction> validskills = new List<BaseAction>();

            validskills = Actor.skills.Where(skills => skills.IsValidAndInRange(this)).ToList();

            if (validskills.Count == 0) return null;

            var random = new System.Random();
            var index = random.Next(validskills.Count);

            return validskills[index];
        }

        public BaseAction ChooseValidSkillAtRandomOrReturnNull()
        {
            List<BaseAction> validskills = new List<BaseAction>();

            validskills = Actor.skills.Where(skills => skills.IsValid()).ToList();

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
        public void ReactionCheck()
        {
            onReactionCheck();
            onReactionCheck = null;
        }
        public void ResetFlags()
        {
            collisionOccured = false;
            foreach (var skill in Actor.skills)
            {
                skill.UpdateCooldown();
            }
            foreach (var reaction in Actor.reactions)
            {
                reaction.UpdateCooldown();
            }

            if(activeStates.OfType<Stagger>().Any())
            {
                Debug.Log("Remove Stagger - Set Posture to max");
                activeStates.Remove(activeStates.OfType<Stagger>().First());
                Actor.currentPosture = Actor.maxPosture;
            }
            else
            {
                Actor.currentPosture += Actor.maxPosture / 8;
            }
            Actor.currentBuildup += 5f;
            //if has posture restore all
            //else restore 1/10 of max
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
                Debug.Log($"Velocity on collision is {rigidbody.velocity.magnitude}");

                UIManager.GetInstance().AddToStoneSlab($"{Actor.Name} hits a wall!");
                onPostureRecieved.Invoke(5f);
                collisionOccured = true;

                // Check if velocity magnitude is greater than the threshold
                if (rigidbody.velocity.magnitude > 0.25f)
                {
                    // Calculate mirrored velocity (mirror along current velocity)
                    Vector3 mirroredVelocity = Vector3.Reflect(rigidbody.velocity, collision.contacts[0].normal);

                    // Replace current velocity with the mirrored velocity
                    rigidbody.velocity = mirroredVelocity * 2f;
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
            PlayAnimation("PostureBroken");
            PlayAudio("Attack1");
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
        public void WaitForAnimation(Action onAnimationEnd)
        {
            //Time.timeScale = 1f;
            //animator.Play(AnimationName, -1, 0);
            //this.onAnimationHitComplete = onAnimationHitComplete;


            this.onAnimationComplete = onAnimationEnd;
        }
        private void OnDestroy()
        {
            GetBaseActor().OnDamageDealt -= ShowDamagePopup;
            GetBaseActor().OnPostureDamage -= ShowPosturePopup;
        }

        public void ApplyPosture(float postureDamage)
        {
            Actor.currentPosture -= postureDamage;

            if (Actor.currentPosture <= Actor.maxPosture / 2)
            {
                var stagger = new Stagger(this);
                activeStates.Add(stagger);
            }

            if (Actor.currentPosture <= 0)
            {
            }
        }

        public void DoDamageRecievedEffects(float damage)
        {
            GetBaseActor().DealDamage(damage);
            ShowDamagePopup(damage);
            PlayAudio("Blow1");
        }

        public void DoPostureRecievedEffects(float damage)
        {
                ApplyPosture(damage);
                ShowPosturePopup(damage);
        }

        public void DoKnockbackRecievedEffects(Vector3 direction, float force)
        {
            ApplyKnockback(direction, force);
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
