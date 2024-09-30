using Assets.Scripts.Battle.Components.AI;
using Assets.Scripts.Battle.Components.Audio;
using Assets.Scripts.Battle.Components.Effects;
using Assets.Scripts.Battle.Components.State;
using Assets.Scripts.Battle.Components.Status;
using Assets.Scripts.Battle.Components.Target;
using System;
using System.Collections;
using UnityEngine;
using static Assets.BaseAction;

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

        public event Action<float> DamageApplied;
        public event Action<float> PostureApplied;
        public event Action<float, Vector3> KnockbackApplied;
        public event Action onAnimationEnd;
        public event Action onAnimationHit;

        // UI
        public OriginPointHandler originPointInUI;

        // Private members
        private bool hitLastRound;
        private Action onReactionCheck;
        private Animator animator;
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

            UIManager.GetInstance().DrawActionAboveHead(this, action);
            UIManager.GetInstance().SetTextThenFade($"{ActorData.Name} uses {action.Name}!", 0.5f);

            action.Perform(this, () => { UIManager.GetInstance().KillActionAboveHead(this); onActionComplete.Invoke(); });
        }
        public void Update()
        {
            state.Update();
        }

        public void ApplyPosture(float originalPostureDamage)
        {
            float postMitigationDamage = originalPostureDamage;
            if (PostureRecieved != null)
            {
                postMitigationDamage = PostureRecieved(originalPostureDamage);
            }

            PostureApplied.Invoke(postMitigationDamage);
            ActorData.DealPostureDamage(postMitigationDamage);

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

            DamageApplied?.Invoke(postMitgationDamage);
            ActorData.DealDamage(postMitgationDamage);

            hitLastRound = true;
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


        public void Move(Vector3 direction, Action onMoveComplete)
        {
            var TargetPosition = transform.position + (direction * ActorData.AGI);

            //state.TransitionTo(new MoveState(this, TargetPosition, onMoveComplete));
        }

        public void PlayAnimation(string AnimationName, Action onAnimationHit, Action onReactionCheck, Action onAnimationEnd)
        {
            animator.Play(AnimationName, -1, 0);
            state.actingState.Locked = false;
            state.TransitionTo(state.actingState.Set(onAnimationEnd, onAnimationEnd, onAnimationHit));
            this.onReactionCheck = onReactionCheck;
        }
        public void PlayAnimation(string AnimationName, Action onAnimationHit, Action onAnimationEnd)
        {
            state.actingState.Locked = false;
            state.TransitionTo(state.actingState.Set(onAnimationEnd, onAnimationEnd, onAnimationHit));
            animator.Play(AnimationName, -1, 0);
        }
        public void PlayAnimation(string AnimationName, Action onAnimationHit, Action onAnimationEnd, bool interruptOnCollision)
        {
            state.actingState.Locked = false;
            state.TransitionTo(state.actingState.Set(onAnimationEnd, onAnimationEnd, onAnimationHit, interruptOnCollision));
            animator.Play(AnimationName, -1, 0);
        }

        public void PlayAnimation(string AnimationName, Action onAnimationEnd)
        {
            state.actingState.Locked = false;
            state.TransitionTo(state.actingState.Set(onAnimationEnd, onAnimationEnd, null));
            animator.Play(AnimationName, -1, 0);
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



        //Do not touch
        public void AnimationHitCallback()
        {
            state.AnimationHitCallback();

            //if (onAnimationHitComplete != null)
            //{
            //    onAnimationHitComplete();
            //    onAnimationHitComplete = null;
            //}
        }
        public void AnimationEndCallback()
        {
            state.AnimationEndCallback();

            //if (onAnimationEndComplete != null)
            //{
            //    onAnimationEndComplete();
            //    onAnimationEndComplete = null;
            //}
        }
        public void ReactionCheck()
        {
            onReactionCheck();
            onReactionCheck = null;
        }

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
    }
}
