using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Components.AI;
using Assets.Scripts.Battle.Components.Audio;
using Assets.Scripts.Battle.Components.Effects;
using Assets.Scripts.Battle.Components.State;
using Assets.Scripts.Battle.Components.Status;
using Assets.Scripts.Battle.Components.Target;
using Assets.Scripts.Utility;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle
{
    public class Actor : MonoBehaviour
    {
        //Data
        public ActorData ActorData;

        //Components
        public ActorStateMachine state;
        public StatusManager statusManager;
        public new AudioManager audio;
        public EffectManager effects;
        public AIComponent ai;
        public TargetingManager target;

        //Events
        public delegate float DamageValue(float damage);
        public delegate float PostureValue(float damage);
        public delegate float KnockbackValue(float force, Vector3 direction);

        public event DamageValue DamageRecieved;
        public event PostureValue PostureRecieved;
        public event KnockbackValue KnockbackRecieved;

        public event Action<float> OnDamageApplied;
        public event Action<float> OnPostureApplied;
        public event Action<float, Vector3> KnockbackApplied;
        public event Action onAnimationEnd;
        public event Action onAnimationHit;

        public Vector3 movementForce;

        // UI
        public OriginPointHandler originPointInUI;

        // Private members
        private bool hitLastRound;
        private Action onReactionCheck;
        private Animator animator;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float moveDuration;
        private float elapsedTime;
        private bool isMoving;
        private LeanTweenType easeType;
        private Action onMoveComplete;
        public float frictionFactor = 3;

        public new Rigidbody rigidbody { get; set; }


        private void Awake()
        {
            rigidbody = gameObject.GetComponent<Rigidbody>();
            animator = gameObject.GetComponent<Animator>();

            state = new ActorStateMachine(this);
            audio = new AudioManager(this);
            statusManager = new StatusManager(this);
            effects = new EffectManager(this);
            ai = new AIComponent(this);
            target = new TargetingManager(this);

            //ActorData.OnDeath += () => { state.TransitionTo(state.idleState); };
        }
        public void Act(Action onActorActionFinished) //Chose and perform an action
        {
            ai.ChooseAction(chosenAction =>
            {
                if (chosenAction == null)
                {
                    //state.TransitionTo(state.actingState.Set(null, null, null));
                    StartCoroutine(WaitForTime(onActorActionFinished, 1f));
                }
                else
                {
                    UseAction(chosenAction, onActorActionFinished);
                    //if (chosenAction.Tags.Contains(TAG.FREE))
                    //{
                    //    UseAction(chosenAction, () => {
                    //        //When the first action finishes
                    //        Act(onActorActionFinished);
                    //    });
                    //}
                    //else
                    //{
                    //    UseAction(chosenAction, onActorActionFinished);
                    //}
                }
            });
        }
        public void UseAction(BaseAction action, Action onActionComplete) //Perform an action
        {
            //Face the enemy
            var lookrotation = target.DirectionToClosestEnemy;
            lookrotation.y = 0;
            transform.rotation = Quaternion.LookRotation(lookrotation, Vector3.up);
            Time.timeScale = 1f;

            UIManager.GetInstance().DrawActionAboveHead(this, action);
            UIManager.GetInstance().SetTextThenFade($"{ActorData.Name} uses {action.Name}!", 0.5f);

            action.Perform(this, () => { UIManager.GetInstance().KillActionAboveHead(this); onActionComplete.Invoke(); });
        }

        public void Update()
        {
            state.Update();
        }

        private void FixedUpdate()
        {
            // Calculate the effective movement based on movementForce
            movementForce = movementForce.normalized * Mathf.Max(0, movementForce.magnitude - frictionFactor * Time.fixedDeltaTime);

            // Only apply force movement if not currently moving towards a target
            if (!isMoving)
            {
                if (movementForce.sqrMagnitude > Mathf.Epsilon)
                {
                    // Move the Rigidbody with background movement
                    rigidbody.MovePosition(rigidbody.position + movementForce * Time.fixedDeltaTime);
                }
            }
            else // Handle easing movement
            {
                // Increment elapsed time by the fixed time step
                elapsedTime += Time.fixedDeltaTime;

                // Calculate the normalized time (0 to 1)
                float t = Mathf.Clamp01(elapsedTime / moveDuration);

                // Apply easing using LeanTween's easing function
                float easedT = StaticHelpers.ApplyEasing(t, easeType);

                // If this is the first frame of the movement, update startPosition
                if (elapsedTime == Time.fixedDeltaTime) // Equivalent to checking if we're starting to move
                {
                    startPosition = rigidbody.position; // Set the start position to the current position
                }

                // Interpolate between the start position and target position using the eased value
                Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, easedT);

                // Move the Rigidbody using MovePosition, combining both movements
                Vector3 combinedMovement = newPosition + movementForce * Time.fixedDeltaTime;
                rigidbody.MovePosition(combinedMovement);

                // If the elapsed time reaches the move duration, stop the movement
                if (elapsedTime >= moveDuration)
                {
                    isMoving = false; // Stop movement
                    elapsedTime = 0f; // Reset elapsed time for next movement
                    onMoveComplete?.Invoke();
                }
            }
        }

        public void ApplyPosture(float originalPostureDamage)
        {
            float postMitigationDamage = originalPostureDamage;
            if (PostureRecieved != null)
            {
                postMitigationDamage = PostureRecieved(originalPostureDamage);
            }

            OnPostureApplied.Invoke(postMitigationDamage);
            var previousPosture = ActorData.currentPosture;
            ActorData.DealPostureDamage(postMitigationDamage);
            var postureLostPercentage = ((previousPosture - ActorData.currentPosture) / ActorData.maxPosture) * 100;
            if (postureLostPercentage > 10)
                if (state.CurrentState == state.actingState)
                {
                    var action = state.actingState.action;
                    if (action is AttackSkill skill)
                    {
                        if (skill.state == AttackSkill.STATE.windup)
                        {
                            float duration = StaticHelpers.LinearMap(postureLostPercentage, 10, 100, 0.3f, 1.5f);
                            state.TransitionTo(state.staggerState.Set(duration));
                        }
                    }
                }


            if (state.CurrentState != state.staggerState && state.CurrentState != state.airStaggerState)
            {
                if (ActorData.currentPosture <= ActorData.maxPosture / 2)
                {
                    state.TransitionTo(state.staggerState);
                }
            }
            else
            {
                //al;ready stagger state

            }

        }
        public void ApplyDamage(float originalDamage)
        {
            float postMitgationDamage = originalDamage;
            if (DamageRecieved != null)
            {
                postMitgationDamage = DamageRecieved(originalDamage);
            }

            OnDamageApplied?.Invoke(postMitgationDamage);
            ActorData.DealDamage(postMitgationDamage);

        }
        public void ApplyKnockback(Vector3 direction, float force)
        {
            var animator = this.GetComponent<Animator>();
            if (force > 0)
            {
                float postMitigationForce = force;

                if (KnockbackRecieved != null)
                {
                    postMitigationForce = KnockbackRecieved(force, direction);
                }

                rigidbody.AddForce(100 * postMitigationForce * direction);
                KnockbackApplied?.Invoke(100 * postMitigationForce, direction);


                //this.MoveToPosition(transform.position + direction.normalized * force, MoveState.Sliding, () => { animator.Play("Idle"); onKnockbackFinished(); });
                //Blood particles
                //GetComponentInChildren<ParticleSystem>().Play();
            }
        }



        public void PlayAnimation(string AnimationName)
        {
            animator.Play(AnimationName);
        }


        public void ProcNextTurnEffects()
        {
            if (ActorData.currentPosture <= ActorData.maxPosture / 2)
            {
                ActorData.currentPosture = ActorData.maxPosture;
                hitLastRound = false;
            }
            else
            {
                if (hitLastRound)
                    hitLastRound = false;
                else
                    ApplyPosture(-5f);
            }
            switch (ActorData.currentStamina / ActorData.maxStamina)
            {
                case float n when (n >= 0 && n < 0.33f):
                    ActorData.DealStaminaDamage(-5f);
                    break;

                case float n when (n >= 0.33 && n < 0.66):
                    ActorData.DealStaminaDamage(-10f);
                    break;

                case float n when (n >= 0.66 && n <= 1):
                    ActorData.DealStaminaDamage(-3f);
                    break;
            }


            foreach (var skill in ActorData.actions)
            {
                skill.UpdateCooldown();
            }
            foreach (var reaction in ActorData.reactions)
            {
                reaction.UpdateCooldown();
            }

            if (state.CurrentState == state.staggerState)
            {
                Debug.Log("Remove Stagger - Set Posture to max");
                state.TransitionTo(state.idleState);
                ActorData.currentPosture = ActorData.maxPosture;
                PlayAnimation("Idle");
            }
        }
        public bool isControllable()
        {
            return ActorData.Controllable;
        }


        public void Move(Vector3 targetPosition, float duration, LeanTweenType easeType, Action onMoveComplete = null)
        {
            // Initialize movement variables
            movementForce = Vector3.zero;
            this.startPosition = rigidbody.position; // Start position is the current position
            this.targetPosition = targetPosition;    // The position to move to
            this.moveDuration = duration;            // The duration of the movement
            this.elapsedTime = 0f;                   // Reset elapsed time
            this.isMoving = true;                    // Flag to start moving
            this.easeType = easeType;                // The easing type for movement
            this.onMoveComplete = onMoveComplete;
        }

        //Do not touch
        public void OnHit()
        {
            state.OnHit();

        }
        public void EnterWindup()
        {
            state.EnterWindup();
        }
        public void EnterRecovery()
        {
            state.EnterRecovery();
        }
        public void OnEnd()
        {
            state.OnEnd();

            //if (onAnimationEndComplete != null)
            //{
            //    onAnimationEndComplete();
            //    onAnimationEndComplete = null;
            //}
        }
        public void ReactionCheck()
        {
            //onReactionCheck();
            //onReactionCheck = null;
        } //Rework so this is no longer needed.

        public System.Collections.IEnumerator WaitForOneFrame(Action action)
        {
            // This will wait for one frame
            yield return new WaitForSeconds(0.15f);

            // Code here will be executed on the frame after the wait
            action.Invoke();
        }
        public System.Collections.IEnumerator WaitForTime(Action action, float seconds)
        {
            // This will wait for one frame
            yield return new WaitForSeconds(seconds);

            // Code here will be executed on the frame after the wait
            action.Invoke();
        }
        private void OnCollisionEnter(Collision collision)
        {
            state.OnCollisionEnter(collision);
        }

        private bool IsGrounded()
        {
            // Perform a raycast from the object's position downward
            Ray ray = new(transform.position + Vector3.up * 0.01f, Vector3.down);

            // Check if the ray hits something within the specified distance
            if (Physics.Raycast(ray, 0.02f))
            {
                return true; // The object is grounded
                //Debug.Log(ActorData.Name + " GROUNDED");
            }
            //Debug.Log(ActorData.Name + " NOT GROUNDED");
            return false; // The object is not grounded
        }

        public bool grounded { get { return IsGrounded(); } private set { } }
        public Animator GetAnimator()
        {
            return animator;
        }
    }
}
