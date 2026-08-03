using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class AirStaggerState : IState
    {
        private Actor actor;
        private SpriteRenderer selectionCircleSR;

        public AirStaggerState(Actor actor)
        {
            this.actor = actor;
            //selectionCircleSR = actor.transform.Find("SelectionCircle").GetComponent<SpriteRenderer>();
        }

        public void Enter()
        {
            actor.PlayAnimation("HurtAir");
        }

        public void Exit()
        {
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Level"))
            {

                // Check if velocity magnitude is greater than the threshold
                if (collision.relativeVelocity.magnitude > 0.1f)
                {
                    var damageInstance = new DamageInstance
                    {
                        Damage = 2f,
                        PostureDamage = 5f,
                    };
                    actor.ApplyDamageInstance(damageInstance, null,null);

                    // Calculate mirrored velocity (mirror along current velocity)
                    Vector3 mirroredVelocity = Vector3.Reflect(actor.Rb.velocity, collision.GetContact(0).normal);
                    var r = collision.relativeVelocity - 2 * Vector3.Dot(actor.Rb.velocity, collision.GetContact(0).normal) * collision.contacts[0].normal;
                    // Replace current velocity with the mirrored velocity
                    actor.Rb.velocity = 2 * r / 3;
                }
            }
        }

        public void Update()
        {
            //var alphaBasedOnHeight = LinearMap(actor.transform.position.y, 0, 3, 0.33f, 0f);
            //selectionCircleSR.color = new Color(selectionCircleSR.color.r, selectionCircleSR.color.g, selectionCircleSR.color.b, alphaBasedOnHeight);
            //selectionCircleSR.transform.position = new Vector3(selectionCircleSR.transform.position.x, 0.01f, selectionCircleSR.transform.position.z);

            if (actor.grounded)
            {
                actor.PlayAnimation("Down");
                actor.state.TransitionTo<GettingUpState>();
            }

            //transition to landing > idle
        }

        static float LinearMap(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((input - inputMin) / (inputMax - inputMin));
        }
    }
}