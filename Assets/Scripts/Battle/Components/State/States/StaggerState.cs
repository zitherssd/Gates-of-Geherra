using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State
{
    public class StaggerState : IState
    {
        private Battle.Actor actor;
        private float idleTimer = 0f;
        private bool collisionOccured;

        public StaggerState(Battle.Actor actor)
        {
            this.actor = actor;
        }

        public void Enter()
        {
            Physics.IgnoreLayerCollision(3, 3, true);
            actor.KnockbackRecieved += ModifyKnockback;
            actor.DamageApplied += ChangeSpriteToDamaged;
            actor.PlayAnimation("PostureBroken");
            actor.audio.PlayAudio("Attack1");
        }

        private float ModifyKnockback(float force, Vector3 direction)
        {
            return force = force * 2;
        }

        private void ChangeSpriteToDamaged(float damage)
        {
            actor.PlayAnimation("HurtGround");
        }

        public void Exit()
        {
            Physics.IgnoreLayerCollision(3, 3, false);
            actor.KnockbackRecieved -= ModifyKnockback;
            actor.DamageApplied -= ChangeSpriteToDamaged;
        }

        public void Update()
        {
            if (!actor.grounded)
                actor.state.TransitionTo(actor.state.airStaggerState);
            if (actor.rigidbody.velocity.magnitude > Mathf.Epsilon)
            {
                idleTimer = 0f;
            }
            else
            {
                // Increment the idle timer if the actor's velocity is zero
                idleTimer += Time.deltaTime;

                // Transition to idleState if the idle timer exceeds 1 second
                if (idleTimer > 0.33f)
                {
                    actor.state.TransitionTo(actor.state.idleState);
                }
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Level"))
            {
                Debug.Log($"Velocity on collision is {actor.rigidbody.velocity.magnitude}");

                if(collision.gameObject.name == "Trap")
                {
                    actor.ApplyDamage(10f);
                    actor.ApplyPosture(5f);
                }
                else
                {
                    actor.ApplyPosture(5f);
                }
                collisionOccured = true;

                // Check if velocity magnitude is greater than the threshold
                if (collision.relativeVelocity.magnitude > 0.1f)
                {
                    // Calculate mirrored velocity (mirror along current velocity)
                    Vector3 mirroredVelocity = Vector3.Reflect(actor.rigidbody.velocity, collision.GetContact(0).normal);
                    var r = collision.relativeVelocity - 2 * Vector3.Dot(actor.rigidbody.velocity, collision.GetContact(0).normal) * collision.contacts[0].normal;

                    // Replace current velocity with the mirrored velocity
                    actor.rigidbody.velocity = r;
                }
            }
        }
    }
}