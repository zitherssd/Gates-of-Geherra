using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State.States
{
    public class LandingState : IState
    {
        private Actor actor;

        public LandingState(Actor actor)
        {
            this.actor = actor;
        }
        public void Enter()
        {
            actor.PlayAnimation("Landing");
        }

        public void Exit()
        {
        }

        public void OnCollisionEnter(Collision collision)
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            if (actor.rigidbody.velocity.sqrMagnitude < Mathf.Epsilon)
                actor.state.TransitionTo(actor.state.idleState);
        }
    }
}