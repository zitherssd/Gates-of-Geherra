using System;
using UnityEngine;

namespace Assets.Scripts.Core
{
    [Serializable]
    public class StateMachine
    {
        [SerializeField]
        public IState CurrentState { get; private set; }

        public event Action<IState> stateChanged;

        public void Initialize(IState state)
        {
            CurrentState = state;
            state.Enter();

            stateChanged?.Invoke(state);
        }

        public void Reset()
        {
            CurrentState = null;
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
    }
}