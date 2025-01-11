using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Components;
using Assets.Scripts.Battle.Components.AI;
using Assets.Scripts.Battle.Components.Audio;
using Assets.Scripts.Battle.Components.Effects;
using Assets.Scripts.Battle.Components.State;
using Assets.Scripts.Battle.Components.Status;
using Assets.Scripts.Battle.Components.Target;
using Assets.Scripts.Utility;
using System;
using Unity.Mathematics;
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
        public MovementManager movement;

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
        public float frictionFactor = 3;
        public bool CanRegenPosture = true;
        public float postureRegenCooldownDuration = 1f;
        private float postureRegenCooldownTimer;
        public float staminaRegenRateModifier = 1.0f;

        public new Rigidbody rigidbody { get; set; }


        private void Awake()
        {
            ActorData.Reset();
            rigidbody = gameObject.GetComponent<Rigidbody>();
            animator = gameObject.GetComponent<Animator>();

            state = new ActorStateMachine(this);
            audio = new AudioManager(this);
            statusManager = new StatusManager(this);
            effects = new EffectManager(this);
            ai = new AIComponent(this);
            target = new TargetingManager(this);
            movement = new MovementManager(this);

        }
        public void UseAction(BaseAction action, Action onActionComplete) //Perform an action
        {
            if(isControllable())
            UIManager.instance.ResetMeter();
            Time.timeScale = 1f;

            //UIManager.GetInstance().DrawActionAboveHead(this, action);

            action.Perform(this, onActionComplete);
        }

        public void Update()
        {
            if(state.CurrentState == state.idleState || state.CurrentState == state.blockState)
            ActorData.DealStaminaDamage(-ActorData.staminaRegenRate * 5 * staminaRegenRateModifier * Time.deltaTime);

            if (!state.IsStaggered() && CanRegenPosture)
            {
                if (ActorData.currentPosture < ActorData.maxPosture)
                {
                    ActorData.DealPostureDamage(-ActorData.postureRegenRate * Time.deltaTime);
                }
            }
            if (!CanRegenPosture && postureRegenCooldownTimer > 0)
            {
                postureRegenCooldownTimer -= Time.deltaTime;
                if (postureRegenCooldownTimer < 0)
                {
                    CanRegenPosture = true;
                }
            }
            foreach (var skill in ActorData.actions)
            {
                skill.UpdateCooldown();
            }
            foreach (var reaction in ActorData.reactions)
            {
                reaction.UpdateCooldown();
            }
            state.Update();
            ai.Update();
            effects.UpdatePolygon();
            if (isControllable()) target.Update();
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
            // Stop posture regeneration and start the cooldown
            CanRegenPosture = false;
            postureRegenCooldownTimer = postureRegenCooldownDuration;


            var postureLostPercentage = (postMitigationDamage / ActorData.maxPosture) * 100;
            if (postureLostPercentage > 10)
                if (state.CurrentState == state.actingState)
                {
                    var action = state.actingState.action;
                    if (action is AttackSkill skill)
                    {
                        if (skill.state == AttackSkill.STATE.windup)
                        {
                            float duration = StaticHelpers.LinearMap(postureLostPercentage, 0, 50, 0, 1f);
                            state.TransitionTo(state.fumbleState.Set(duration));
                        }
                    }
                }
            if (ActorData.currentPosture <= ActorData.maxPosture / 2)
            {
                //inflict stagger or if already staggered, refresh duration
                float duration = StaticHelpers.LinearMap(postureLostPercentage, 0, 100, 1f, 4f);
                state.TransitionTo(state.staggerState.Set(duration));
            }

        }
        public void ApplyDamage(float originalDamage)
        {
            ActorData.ChangeBuildup(Mathf.Max(UnityEngine.Random.Range(2,3), originalDamage));
            float postMitgationDamage = originalDamage;
            if (DamageRecieved != null)
            {
                postMitgationDamage = DamageRecieved(originalDamage);
            }

            OnDamageApplied?.Invoke(postMitgationDamage);
            ActorData.DealDamage(postMitgationDamage);
            if (isControllable()) UIManager.instance.GainMeter(0.5f);
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
                if (!state.IsStaggered()) direction.y = 0;
                movement.AddForce(postMitigationForce * direction);
                KnockbackApplied?.Invoke(postMitigationForce, direction);


                //this.MoveToPosition(transform.position + direction.normalized * force, MoveState.Sliding, () => { animator.Play("Idle"); onKnockbackFinished(); });
                //Blood particles
                //GetComponentInChildren<ParticleSystem>().Play();
            }
        }



        public void PlayAnimation(string AnimationName)
        {
            animator.Play(AnimationName);
        }

        public bool isControllable()
        {
            return ActorData.Controllable;
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
