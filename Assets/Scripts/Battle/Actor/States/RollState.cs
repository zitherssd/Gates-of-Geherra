using Assets.Scripts.Pattern;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class RollState : IState
    {
        private Actor actor;
        private SpriteRenderer selectionCircleSR;

        public RollState(Actor actor)
        {
            this.actor = actor;
            selectionCircleSR = actor.transform.Find("SelectionCircle").GetComponent<SpriteRenderer>();
        }

        public void Enter()
        {
            actor.PlayAnimation("RollBackwards");
        }

        public void Exit()
        {
        }

        public void Update()
        {
            var alphaBasedOnHeight = LinearMap(actor.transform.position.y, 0, 3, 0.33f, 0f);
            selectionCircleSR.color = new Color(selectionCircleSR.color.r, selectionCircleSR.color.g, selectionCircleSR.color.b, alphaBasedOnHeight);
            selectionCircleSR.transform.position = new Vector3(selectionCircleSR.transform.position.x, 0.01f, selectionCircleSR.transform.position.z);


            if (actor.grounded)
            {
                actor.state.TransitionTo<LandingState>();
            }
            else if (actor.Rb.velocity.sqrMagnitude < 0.1f)
            {
                //actor.state.TransitionTo(actor.state.airNeutralState);
            }
        }
        static float LinearMap(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((input - inputMin) / (inputMax - inputMin));
        }

        public void OnCollisionEnter(Collision collision)
        {
        }
    }
}