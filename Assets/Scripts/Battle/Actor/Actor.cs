using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Actions;
using Assets.Scripts.Battle.Actor.AI;
using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Battle.Actor.Systems;
using Assets.Scripts.Battle.Components.Audio;
using Assets.Scripts.Battle.Components.Effects;
using Assets.Scripts.Battle.Components.Status;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Utility;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Battle.Actor
{
    [RequireComponent(typeof(StatusManager))]
    public class Actor : MonoBehaviour
    {
        //Data
        [FormerlySerializedAs("ActorData")]
        public ActorDefinition Definition;
        public ActorRuntime Runtime { get; set; }

        //Components
        public ActorStateMachine state;
        public StatusManager statusManager;
        public new AudioManager audio; //this too?
        public EffectManager effects; //this can be moved to a different monobehaviour
        public AIBT ai; //ai
        public TargetingSystem target; //this needs to be reworked? we should calculate this kind of stuff on demand?
        public MovementSystem movement; //what is tihs?
        public ActorInventory inventory;

        // --- New Event System ---
        public event Action<DamageInstance> OnBeforeTakeDamage;
        public event Action<DamageInstanceResult> OnAfterTakeDamage;
        public event Action<DamageInstance> OnBeforeDealDamage;
        public event Action<DamageInstance> OnAfterDealDamage;
        public event Action<BaseAction> OnActionUsed;
        // ------------------------

        public event Action OnReset;

        // Private members
        private Animator animator;
        private float postureRegenCooldownTimer;
        private BaseAction _currentAction;

        // Public properties
        public bool CanRegenPosture = true;
        public float PostureRegenCooldownDelay = 1f;
        public float StaminaRegenRate = 1.0f;
        public float PostureREgenRate = 1.0f;
        public bool isControllable { get { return Definition != null && Definition.Controllable; } private set { } }

        public BaseAction GetCurrentAction() => _currentAction;

        // Public methods
        public void Awake()
        {
            Rb = gameObject.GetComponent<Rigidbody>();
            animator = gameObject.GetComponent<Animator>();
            state = gameObject.GetComponent<ActorStateMachine>();
            audio = new AudioManager(this);
            statusManager = gameObject.GetComponent<StatusManager>();
            effects = new EffectManager(this);
            target = new TargetingSystem(this); //this too subscribe
            movement = new MovementSystem(this); //this too subscribe???
            inventory = new ActorInventory(this);
        }

        public void Start()
        {
            ai = new AIBT(this); //this too subscribe
            EnsureRuntime();
            state.Initialize<InactiveState>();
            effects.SetColors();
            foreach (var item in Runtime.items)
                inventory.AddItem(item);
        }

        //Should run after 
        public void Spawn()
        {
            EnsureRuntime();
            Runtime.ResetToDefinition();
        }

        public void Update()
        {
            if (BattleManager.instance.enabled == false) return;
            if (Runtime == null) return;

            StaminaRegen();
            PostureRegen();
            UpdateActionCooldowns();
            statusManager.Tick();

            //subcomponents update
            ai.Update();
            effects.UpdatePolygon();
            if (isControllable) target.Update(); //this looks wierd
        }

        void FixedUpdate()
        {
            movement.agent.nextPosition = transform.position;
        }

        public void Init()
        {
            EnsureRuntime();
            effects.SetColors();
            //Do other one time things when spawning
        }

        public void SetDefinition(ActorDefinition definition)
        {
            Definition = definition;

            if (Runtime == null)
            {
                Runtime = new ActorRuntime(Definition);
            }
            else
            {
                Runtime.Definition = Definition;
                Runtime.ResetToDefinition();
            }
        }

        private void EnsureRuntime()
        {
            if (Runtime == null && Definition != null)
            {
                Runtime = new ActorRuntime(Definition);
            }
        }


        public void UseAction(BaseAction action)
        {
            if (_currentAction != null && _currentAction != action)
            {
                _currentAction.EndAction(ActionEndReason.Interrupted);
            }

            if (isControllable && SlowdownManager.instance != null)
                SlowdownManager.instance.ResetStateSlowdown();

            _currentAction = action;
            _currentAction.OnActionEnded += OnActionEnded;
            OnActionUsed?.Invoke(action);
            _currentAction.Begin(this);
        }

        private void OnActionEnded(BaseAction action, ActionEndReason reason)
        {

            // Unsubscribe from the action that ended.
            action.OnActionEnded -= OnActionEnded;

            // Only null out _currentAction and transition if the action that
            // ended is the one we currently care about.
            if (_currentAction == action)
            {
                _currentAction = null;
                state.TransitionToIdle();
            }
        }


        public void ApplyDamageInstance(DamageInstance damageInstance, Actor attacker, BaseAction action = null)
        {
            DamageInstanceResult result = damageInstance.Calculate(attacker, this, action);
            var damage = result.DamageDealt;
            var postureDamage = result.PostureDamageDealt;
            var force = result.KnockbackForceApplied;
            var direction = result.KnockbackApplied;

            // 1. Invoke OnBeforeTakeDamage event, allowing listeners to modify the damage.
            OnBeforeTakeDamage?.Invoke(damageInstance);

            //Damage
            #region damage
            //Changes buildup by 1% for each 2% of hp lost? not yet
            Runtime.ChangeBuildup(Mathf.Max(UnityEngine.Random.Range(2, 3), damage));

            ItemEventBus.Raise(ItemTrigger.OnDamageTaken, this);
            Runtime.DealDamage(damage);
            var DiedFromThisHit = Runtime.isDead();
            //
            #endregion

            if (DiedFromThisHit)
            {
                CameraManager.instance.SlowTrack = true;
                state.GetState<StaggerState>().Set(2f);
                state.TransitionTo<StaggerState>();

                if (force > 0f)
                {
                    var newForce = force * 1.2f;
                    var newDirection = direction;
                    newDirection.y += 0.6f;
                    movement.AddForce(newForce * newDirection);
                }
            }
            else
            {
                //Posture
                var previousPosture = Runtime.currentPosture;
                Runtime.DealPostureDamage(postureDamage);
                // Stop posture regeneration and start the cooldown
                CanRegenPosture = false;
                postureRegenCooldownTimer = PostureRegenCooldownDelay;


                var postureLostPercentage = (Mathf.Min(postureDamage, previousPosture) / Runtime.maxPosture) * 100;
                if (Runtime.currentPosture <= Runtime.maxPosture / 2)
                {
                    // Calculate stagger duration
                    float staggerDuration = StaticHelpers.LinearMap(Mathf.Min(postureLostPercentage), 0, 100, 1f, 2.5f);
                    state.GetState<StaggerState>().Set(staggerDuration);
                    state.TransitionTo<StaggerState>();
                }

                if (force > 0)
                {
                    if (!state.IsStaggered())
                        direction.y = 0;

                    movement.AddForce(force * direction);
                }
            }

            // 2. Invoke OnAfterTakeDamage event
            OnAfterTakeDamage?.Invoke(result);

            // Trigger temporary slowdown effect on damage
            if (isControllable && SlowdownManager.instance != null)
                SlowdownManager.instance.TriggerTemporarySlowdown(0.4f, new AnimationCurve());
        }

        public void DealDamage(DamageInstance damageInstance, Actor targetActor, BaseAction action)
        {
            // 1. Invoke OnBeforeDealDamage event, allowing listeners to modify the damage.
            OnBeforeDealDamage?.Invoke(damageInstance);

            // 2. Apply the damage to the target.
            targetActor.ApplyDamageInstance(damageInstance, this, action);

            // 3. Invoke OnAfterDealDamage event.
            OnAfterDealDamage?.Invoke(damageInstance);
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
                //Debug.Log(Runtime.Name + " GROUNDED");
            }
            //Debug.Log(Runtime.Name + " NOT GROUNDED");
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
                if (Runtime.currentPosture < Runtime.maxPosture)
                {
                    Runtime.DealPostureDamage(-Runtime.postureRegenRate * Time.deltaTime);
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
            // Regen when in idle state
            if (state.IsIdle())
            {
                Runtime.DealStaminaDamage(-Runtime.staminaRegenRate * 5 * StaminaRegenRate * Time.deltaTime);
                return;
            }

            // Also regen when standing still during movement (joystick in deadzone)
            if (state.CurrentState is MoveState moveState)
            {
                Runtime.DealStaminaDamage(-Runtime.staminaRegenRate * 5 * StaminaRegenRate * Time.deltaTime * (1 - moveState.action.Direction.magnitude));
            }
        }
        private void UpdateActionCooldowns()
        {
            foreach (var skill in Runtime.actions)
            {
                skill.UpdateCooldown();
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
