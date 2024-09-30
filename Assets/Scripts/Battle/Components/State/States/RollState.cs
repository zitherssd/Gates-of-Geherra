using UnityEditor;
using UnityEngine;
using Assets.Scripts.Pattern;


namespace Assets.Scripts.Battle.Components.State.States
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
            actor.PlayAnimation("Roll");
        }

        public void Exit()
        {
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Level"))
            {

                if (collision.gameObject.name == "LandingTrap")
                {
                    Debug.Log($"{actor.ActorData.name} landed on LandingTrap");

                    actor.ApplyDamage(10f);
                    actor.ApplyPosture(5f);
                }
            }
        }

        public void Update()
        {
            var alphaBasedOnHeight = LinearMap(actor.transform.position.y, 0, 3, 0.33f, 0f);
            selectionCircleSR.color = new Color(selectionCircleSR.color.r, selectionCircleSR.color.g, selectionCircleSR.color.b, alphaBasedOnHeight);
            selectionCircleSR.transform.position = new Vector3(selectionCircleSR.transform.position.x, 0.01f, selectionCircleSR.transform.position.z);


            if (actor.grounded)
            {
                actor.state.TransitionTo(actor.state.landingState);
            }
            else if (actor.rigidbody.velocity.sqrMagnitude < 0.1f)
            {
                //actor.state.TransitionTo(actor.state.airNeutralState);
            }
        }
        static float LinearMap(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((input - inputMin) / (inputMax - inputMin));
        }
    }
}