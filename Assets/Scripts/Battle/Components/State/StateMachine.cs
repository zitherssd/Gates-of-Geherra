using Assets.Scripts.Battle.Components.State.States;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State
{
    [Serializable]
    public class StateMachine
    {
        [SerializeField]
        public IState CurrentState { get; private set; }

        public event Action<IState> stateChanged;

        public IdleState idleState;
        public StaggerState staggerState;
        public AirStaggerState airStaggerState;
        public AirNeutralState airNeutralState;
        public LandingState landingState;
        public ActingState actingState;
        public RollState rollState;
        public BlockState blockState;
        public MoveState moveState;

        public StateMachine(Actor actor)
        {
            this.idleState = new IdleState(actor);
            this.staggerState = new StaggerState(actor);
            this.airStaggerState = new AirStaggerState(actor);
            this.landingState = new LandingState(actor);
            this.airNeutralState = new AirNeutralState(actor);
            this.actingState = new ActingState(actor);
            this.rollState = new RollState(actor);
            this.blockState = new BlockState(actor);
            this.moveState = new MoveState(actor);
            Initialize(idleState);
        }

        public void Initialize(IState state)
        {
            CurrentState = state;
            state.Enter();

            stateChanged?.Invoke(state);
        }

        public void TransitionTo(IState nextState)
        {
            if (CurrentState == nextState) return;
            CurrentState.Exit();
            CurrentState = nextState;
            nextState.Enter();

            stateChanged?.Invoke(nextState);
        }

        public void Update()
        {
            if (CurrentState != null)
                CurrentState.Update();
        }

        internal void OnCollisionEnter(Collision collision)
        {
            if (CurrentState != null)
                CurrentState.OnCollisionEnter(collision);
        }
    }
}