using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class AirNeutralState : IState
    {
        private Actor actor;
        private SpriteRenderer selectionCircleSR;

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
            //throw new System.NotImplementedException();
        }

        public void Update()
        {
            //var alphaBasedOnHeight = LinearMap(actor.transform.position.y, 0, 3, 0.33f, 0f);
            //selectionCircleSR.color = new Color(selectionCircleSR.color.r, selectionCircleSR.color.g, selectionCircleSR.color.b, alphaBasedOnHeight);
            //selectionCircleSR.transform.position = new Vector3(selectionCircleSR.transform.position.x, 0.01f, selectionCircleSR.transform.position.z);

            if (actor.grounded)
            {
                if(Mathf.Abs(actor.Rb.velocity.x) > 0.05f)
                {
                    actor.state.TransitionTo<LandingState>();
                }
                else
                {
                    actor.state.TransitionToIdle();
                    actor.PlayAnimation("Idle");
                }
            }

            //transition to landing > idle
        }

        static float LinearMap(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((input - inputMin) / (inputMax - inputMin));
        }
    }
}