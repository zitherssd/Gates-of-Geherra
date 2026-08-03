using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.CullingGroup;

namespace Assets.Scripts.Battle.Actor
{
    public class ActorStateMachine : MonoBehaviour
    {

        //Public fields
        public IState CurrentState { get; private set; }
        public event Action<IState> StateChanged;
        public bool locked;


        //Private fields
        private readonly Dictionary<Type, IState> states = new();
        private Actor actor;
        [Header("Debug")]
        [SerializeField] private string currentStateName;
        public T GetState<T>() where T : class, IState
        {
            if (states.TryGetValue(typeof(T), out var state))
            {
                return state as T;
            }

            Debug.LogWarning($"State of type {typeof(T).Name} not found in the state machine.");
            return null;
        }

        //Public methods
        public void Awake()
        {
            actor = GetComponent<Actor>();

            RegisterState(new IdleState(actor));
            RegisterState(new InactiveState(actor));
            RegisterState(new FumbleState(actor));
            RegisterState(new AirStaggerState(actor));
            RegisterState(new AirNeutralState(actor));
            RegisterState(new LandingState(actor));
            RegisterState(new ActingState(actor));
            RegisterState(new RollState(actor));
            RegisterState(new BlockState(actor));
            RegisterState(new MoveState(actor));
            RegisterState(new GettingUpState(actor));
            RegisterState(new StaggerState(actor));
            RegisterState(new DeathState(actor));
        }
        public void Initialize<T>() where T : IState
        {
            if (!states.TryGetValue(typeof(T), out var nextState))
            {
                Debug.LogError($"State {typeof(T).Name} not registered!");
                return;
            }

            CurrentState = nextState;
            CurrentState.Enter();

            StateChanged?.Invoke(CurrentState);
        }
        public void Update()
        {
            if (CurrentState != null)
                CurrentState.Update();
        }
        public void OnHit()
        {
            if (CurrentState is ActingState actingState)
            {
                actingState.OnHit();
            }
        }
        public void OnEnd()
        {
            if (CurrentState is ActingState actingState)
            {
                actingState.OnEnd();
            }
        }
        public void EnterWindup(int windupFrames)
        {
            if (CurrentState is ActingState actingState)
            {
                actingState.EnterWindup(windupFrames);
            }
        }
        public void EnterRecovery()
        {
            if (CurrentState is ActingState actingState)
            {
                actingState.EnterRecovery();
            }
        }
        public void OnCollisionEnter(Collision collision)
        {
            if (CurrentState != null)
                CurrentState.OnCollisionEnter(collision);
        }


        //Helper methods
        public void TransitionToIdle()
        {
                if (!IsAlive())
                {
                    TransitionTo<DeathState>();
                }
                else
                {
                    TransitionTo<IdleState>();
                }
        }
        public bool IsIdle()
        {
            if (Is<IdleState>())
                return true;
            else
                return false;
        }
        public bool IsKnockedDown()
        {
            if (Is<GettingUpState>())
                return true;
            else
                return false;
        }
        public bool IsAlive()
        {
            if (!Is<DeathState>())
                return true;
            else
                return false;
        }
        public bool IsStaggered()
        {
            if (CurrentState is StaggerState staggerState || CurrentState is FumbleState fumbleState || CurrentState is AirStaggerState airStaggerState)
                return true;
            else return false;
        }
        public bool IsBlocking()
        {
            if (CurrentState is BlockState)
                return true;
            else return false;
        }
        public bool IsMoving(out MoveAction moveAction)
        {
            moveAction = null;
            if (CurrentState is MoveState moveState)
            {
                moveAction = moveState.action as MoveAction;
                return true;
            }
            else
                return false;
        }
        public bool IsAttacking(out AttackSkill attackSkill)
        {
            if (CurrentState is ActingState actingState && actingState.action is AttackSkill skill)
            {
                attackSkill = skill;
                if (attackSkill.state != AttackSkill.STATE.windup)
                    return false;
                else
                    return true;
            }
            else
            {
                attackSkill = null;
                return false;
            }
        }
        private void RegisterState(IState state)
        {
            states[state.GetType()] = state;
        }


        public T TransitionTo<T>() where T : class, IState
        {
            if (!states.TryGetValue(typeof(T), out var nextState))
            {
                Debug.LogError($"[{actor.name}] State of type {typeof(T)} not found!");
                return default;
            }

            if (nextState == null)
            {
                Debug.LogError($"[{actor.name}] NextState for {typeof(T)} is NULL!");
                return default;
            }

            if (CurrentState == nextState)
                return CurrentState as T;


            CurrentState.Exit();
            CurrentState = nextState;
            currentStateName = nextState.GetType().Name;
            nextState.Enter();

            if (!(CurrentState is T))
            {
                Debug.LogError($"Invalid cast! Expected {typeof(T).Name} but CurrentState is {CurrentState.GetType().Name}");
            }

            return CurrentState as T;
        }
        public bool Is<T>() where T : IState => CurrentState is T;
    }
}