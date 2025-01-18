using Assets.Scripts.Pattern;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State.States
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
            if (duration > this.duration)
                this.duration = duration;
            return this;
        }

        public void Enter()
        {
            if (actor.isControllable())
            {
                UIManager.GetInstance().HideUI();
                var Ready = false;
            }
            Physics.IgnoreLayerCollision(3, 3, true);
            //actor.KnockbackRecieved += ModifyKnockback;
            actor.PlayAnimation("HurtGround");
            actor.audio.PlayAudio("Attack1");
            actor.movement.SetFriction(0.33f);
            actor.KnockbackRecieved += Actor_KnockbackApplied;
            collisionOccured = false;
            timer = 0f;
        }

        private float Actor_KnockbackApplied(float arg1, Vector3 arg2)
        {
            return arg1 * 2;
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
            actor.KnockbackRecieved -= Actor_KnockbackApplied;
            actor.movement.SetFriction();
        }

        public void Update()
        {
            if (!actor.grounded)
            {
                actor.state.TransitionTo(actor.state.airStaggerState);
                return;
            }

            // Increment the timer by deltaTime
            timer += Time.deltaTime;

            // Check if the total stagger duration has been reached
            if (timer >= duration)
            {
                Debug.Log($"Timer reached duration: {timer}, Duration: {duration}");
                actor.ActorData.currentPosture = actor.ActorData.maxPosture;
                actor.state.TransitionTo(actor.state.idleState);  // Transition back to idle state
            }
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