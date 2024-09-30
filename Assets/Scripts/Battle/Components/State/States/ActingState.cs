using Assets.Scripts.Pattern;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State.States
{
    public class ActingState : IState
    {
        private readonly Actor owner;
        public bool completetest;
        public Action onActionHit;
        private bool interruptOnCollision;
        public Action onActionInterrupt;
        public Action onActionComplete;
        public bool Locked = false;

        public ActingState(Actor owner)
        {
            this.owner = owner;
        }

        public ActingState Set(Action onActionInterrupt, Action onActionComplete, Action onActionHit)
        {
            this.onActionInterrupt = onActionInterrupt;
            this.onActionComplete = onActionComplete;
            this.onActionHit = onActionHit;
            interruptOnCollision = false;
            completetest = false;
            return this;
        }

        public ActingState Set(Action onActionInterrupt, Action onActionComplete, Action onActionHit, bool interruptOnCollision)
        {
            this.onActionInterrupt = onActionInterrupt;
            this.onActionComplete = onActionComplete;
            this.onActionHit = onActionHit;
            this.interruptOnCollision = interruptOnCollision;
            completetest = false;
            return this;
        }

        public void Enter()
        {
        }

        public void Exit()
        {
            if (!completetest)
                onActionInterrupt?.Invoke();
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (interruptOnCollision)
                Exit();
        }

        public void Update()
        {

        } 
    }
}