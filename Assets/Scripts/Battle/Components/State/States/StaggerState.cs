using Assets.Scripts.Pattern;
using UnityEngine;


namespace Assets.Scripts.Battle.Components.State
{
    public class StaggerState : IState
    {
        private Battle.Actor actor;
        private float timer = 0f;
        private bool collisionOccured;
        private float duration = 0f;

        public StaggerState(Battle.Actor actor)
        {
            this.actor = actor;
        }

        public StaggerState Set(float duration)
        {
            this.duration = duration;
            return this;
        }

        public void Enter()
        {
            Physics.IgnoreLayerCollision(3, 3, true);
            //actor.KnockbackRecieved += ModifyKnockback;
            actor.PlayAnimation("PostureBroken");
            actor.audio.PlayAudio("Attack1");
            actor.rigidbody.drag = 0.15f;
            collisionOccured = false;
            timer = 0f;
        }

        private void ChangeSpriteToDamaged(float damage)
        {
            actor.PlayAnimation("HurtGround");
        }

        public void Exit()
        {
            Physics.IgnoreLayerCollision(3, 3, false);
            //actor.KnockbackRecieved -= ModifyKnockback;
            actor.OnDamageApplied -= ChangeSpriteToDamaged;
            actor.rigidbody.drag = 0.33f;
            timer = 0f;
            actor.ActorData.currentPosture = actor.ActorData.maxPosture;

        }

        public void Update()
        {
            timer += Time.deltaTime;
            if (timer > duration)
                actor.state.TransitionTo(actor.state.idleState);
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Level"))
            {
                Debug.Log($"Velocity on collision is {actor.rigidbody.velocity.magnitude}");
                Debug.Log($"RelativeVel is {collision.relativeVelocity.magnitude}");
                if (collisionOccured == false)
                {
                    if (collision.gameObject.name == "Trap")
                    {
                        Debug.Log($"{actor.ActorData.name} hit trap!");
                        actor.ApplyDamage(10f);
                        actor.ApplyPosture(5f);
                    }
                    else
                    {
                        actor.ApplyPosture(5f);
                    }
                }

                collisionOccured = true;

                // Check if velocity magnitude is greater than the threshold
                if (collision.relativeVelocity.magnitude > 0.1f)
                {
                    // Calculate mirrored velocity (mirror along current velocity)
                    Vector3 mirroredVelocity = Vector3.Reflect(actor.rigidbody.velocity, collision.GetContact(0).normal);
                    var r = collision.relativeVelocity - 2 * Vector3.Dot(actor.rigidbody.velocity, collision.GetContact(0).normal) * collision.contacts[0].normal;

                    // Replace current velocity with the mirrored velocity
                    actor.rigidbody.velocity = 2 * r / 3;
                }
            }
        }
    }
}