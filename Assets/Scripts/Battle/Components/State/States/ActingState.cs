using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State.States
{
    public class ActingState : IState
    {
        private readonly Actor owner;
        private bool complete;
        private Action onActionInterrupt;
        private Action onActionComplete;

        public ActingState(Actor owner)
        {
            this.owner = owner;
        }

        public ActingState Set(Action onActionInterrupt, Action onActionComplete)
        {
            this.onActionInterrupt = onActionInterrupt;
            this.onActionComplete = onActionComplete;
            complete = false;
            return this;
        }

        public void Enter()
        {
        }

        public void Exit()
        {
            if (complete)
                onActionComplete?.Invoke();
            else
                onActionInterrupt?.Invoke();
        }

        public void OnCollisionEnter(Collision collision)
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
        }
    }
}