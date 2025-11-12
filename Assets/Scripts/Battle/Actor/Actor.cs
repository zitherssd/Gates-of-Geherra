using System;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor.AI;
using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Battle.Actor.Systems;
using Assets.Scripts.Battle.Components.Audio;
using Assets.Scripts.Battle.Components.Effects;
using Assets.Scripts.Battle.Components.Status;
using Assets.Scripts.Utility;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace Assets.Scripts.Battle.Actor
{
    public class Actor : MonoBehaviour
    {
        //Data
        public ActorData ActorData;

        //Components
        public ActorStateMachine state;
        public StatusManager statusManager;
        public new AudioManager audio; //this too?
        public EffectManager effects; //this can be moved to a different monobehaviour
        public AIBT ai; //ai
        public TargetingSystem target; //this needs to be reworked? we should calculate this kind of stuff on demand?
        public MovementSystem movement; //what is tihs?

        //Delegates
        public delegate float DamageValue(float damage);
        public delegate float PostureValue(float damage);
        public delegate float KnockbackValue(float force, Vector3 direction);

        //Events
        public event DamageValue DamageRecieved;
        public event PostureValue PostureRecieved;
        public event KnockbackValue KnockbackRecieved;

        public event Action<float> DamageApplied;
        public event Action<float> PostureApplied;
        public event Action<float, Vector3> KnockbackApplied;

        public event Action OnReset;

        // Private members
        private Animator animator;
        private float postureRegenCooldownTimer;

        // Public properties
        public bool CanRegenPosture = true;
        public float PostureRegenCooldownDelay = 1f;
        public float StaminaRegenRate = 1.0f;
        public bool isControllable { get { return ActorData.Controllable; } private set { }}

        // Public methods
        public void Awake()
        {
            Rb = gameObject.GetComponent<Rigidbody>();
            animator = gameObject.GetComponent<Animator>();
            state = gameObject.GetComponent<ActorStateMachine>();
            audio = new AudioManager(this);
            statusManager = new StatusManager(this); //this needs rework
            effects = new EffectManager(this);
            target = new TargetingSystem(this); //this too subscribe
            movement = new MovementSystem(this); //this too subscribe???

        }

        public void Start()
        {
            if (ActorData != null)
            ActorData.Reset();
            ai = new AIBT(this); //this too subscribe
            state.Initialize<IdleState>();
            effects.SetColors();
        }

        public void Update()
        {
             StaminaRegen();
            PostureRegen();
            UpdateActionCooldowns();

            //subcomponents update
            ai.Update();
            effects.UpdatePolygon();
            if (isControllable) target.Update(); //this looks wierd
        }

        public void Init()
        {
            effects.SetColors();
        }


        public void UseAction(BaseAction action, Action onActionComplete) 
        {
            if (isControllable)
                UIManager.instance.ResetMeter();
            Time.timeScale = 1f;


            action.Perform(this, onActionComplete);
        }
        public void ApplyDamage(float originalDamage)
        {
            ActorData.ChangeBuildup(Mathf.Max(UnityEngine.Random.Range(2, 3), originalDamage));
            float postMitgationDamage = originalDamage;
            if (DamageRecieved != null)
            {
                postMitgationDamage = DamageRecieved(originalDamage);
            }

            DamageApplied?.Invoke(postMitgationDamage);
            ActorData.DealDamage(postMitgationDamage);
            if (isControllable) UIManager.instance.GainMeter(0.5f);
        }

        public void ApplyPosture(float originalPostureDamage)
        {
            float postMitigationDamage = originalPostureDamage;
            if (PostureRecieved != null)
            {
                postMitigationDamage = PostureRecieved(originalPostureDamage);
            }

            PostureApplied.Invoke(postMitigationDamage);
            var previousPosture = ActorData.currentPosture;
            ActorData.DealPostureDamage(postMitigationDamage);
            // Stop posture regeneration and start the cooldown
            CanRegenPosture = false;
            postureRegenCooldownTimer = PostureRegenCooldownDelay;


            var postureLostPercentage = (Mathf.Min(postMitigationDamage, previousPosture) / ActorData.maxPosture) * 100;
            if (ActorData.currentPosture <= ActorData.maxPosture / 2)
            {
                // Calculate stagger duration
                float staggerDuration = StaticHelpers.LinearMap(Mathf.Min(postureLostPercentage), 0, 100, 1f, 2.5f);
                state.GetState<StaggerState>().Set(staggerDuration);
                state.TransitionTo<StaggerState>();
                return;
            }

            if (ActorData.isDead())
                state.TransitionTo<StaggerState>().Set(2f);

        }
        public void ApplyKnockback(Vector3 direction, float force)
        {
            if (force > 0)
            {
                float postMitigationForce = force;

                if (KnockbackRecieved != null)
                {
                    postMitigationForce = KnockbackRecieved(force, direction);
                }

                if(!ActorData.isDead())
                {
                    if (!state.IsStaggered())
                        direction.y = 0;
                }
                else
                {
                    direction = direction * 1.2f;
                    if (direction.y < 0.3f) direction.y = 0.3f;
                }
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



        // Move this to movement
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
        } //rework


        //Private methods
        private void PostureRegen()
        {
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
        }
        private void StaminaRegen()
        {
            if (state.IsIdle())
                ActorData.DealStaminaDamage(-ActorData.staminaRegenRate * 5 * StaminaRegenRate * Time.deltaTime);
        }
        private void UpdateActionCooldowns()
        {
            foreach (var skill in ActorData.actions)
            {
                skill.UpdateCooldown();
            }
            foreach (var reaction in ActorData.reactions)
            {
                reaction.UpdateCooldown();
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (state != null)
                state.OnCollisionEnter(collision);
        }
        
        
        // Public components
        public Rigidbody Rb { get; set; }
    }
}
