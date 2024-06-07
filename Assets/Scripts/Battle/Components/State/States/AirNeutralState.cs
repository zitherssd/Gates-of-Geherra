using UnityEngine;
namespace Assets.Scripts.Battle.Components.State.States
{
    public class AirNeutralState : IState
    {
        private Actor actor;

        public AirNeutralState(Actor actor)
        {
            this.actor = actor;
        }

        public void Enter()
        {
            actor.PlayAnimation("NeutralAir");
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
            if (actor.grounded)
            {
                if(Mathf.Abs(actor.rigidbody.velocity.x) > 0.05f)
                {
                    actor.state.TransitionTo(actor.state.landingState);
                }
                else
                {

                }
                actor.state.TransitionTo(actor.state.idleState);
            }

            //transition to landing > idle
        }
    }
}