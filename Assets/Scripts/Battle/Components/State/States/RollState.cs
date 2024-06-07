using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State.States
{
    public class RollState : IState
    {
        private Actor actor;

        public RollState(Actor actor)
        {
            this.actor = actor;
        }

        public void Enter()
        {
            actor.PlayAnimation("Roll");
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
            if(actor.grounded)
            {
                actor.state.TransitionTo(actor.state.landingState);
            }
            else if (actor.rigidbody.velocity.sqrMagnitude < 0.1f)
            {
                //actor.state.TransitionTo(actor.state.airNeutralState);
            }
        }
    }
}