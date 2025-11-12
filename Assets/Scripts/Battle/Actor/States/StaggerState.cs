using System;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Pattern;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class StaggerState : IState
    {
        private Actor actor;
        private float timer = 0f;
        private bool collisionOccured;
        private float duration = 0f;

        public event Action<float> OnStaggerStateEntered;
        public event Action OnStaggerStateExit;

        public StaggerState(Actor actor)
        {
            this.actor = actor;
        }

        public StaggerState Set(float duration)
        {
            if (duration > this.duration)
                this.duration = duration;
            return this;
        }
        private void DeathCheck()
        {
            if (actor.ActorData.isDead())
            {
                actor.state.TransitionTo<DeathState>();
                BattleManager.instance.End();
                return;
            }
        }
        public void Enter()
        {
            OnStaggerStateEntered?.Invoke(duration);
            if (actor.isControllable)
            {
                UIManager.GetInstance().HideUI();
                var Ready = false;
            }

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
            return arg1 * 1.5f;
        }

        private void ChangeSpriteToDamaged(float damage)
        {
            actor.PlayAnimation("HurtGround");
        }

        public void Exit()
        {
            OnStaggerStateExit?.Invoke();
            //actor.KnockbackRecieved -= ModifyKnockback;
            actor.DamageApplied -= ChangeSpriteToDamaged;
            actor.KnockbackRecieved -= Actor_KnockbackApplied;
            actor.movement.SetFriction();
        }

        public void Update()
        {
            if (!actor.grounded)
            {
                actor.state.TransitionTo<AirStaggerState>();
                return;
            }

            // Increment the timer by deltaTime
            timer += Time.deltaTime;

            // Check if the total stagger duration has been reached
            if (timer >= duration)
            {
                actor.ActorData.currentPosture = actor.ActorData.maxPosture;
                actor.state.TransitionToIdle();  // Transition back to idle state
                actor.PlayAnimation("Idle");
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Level"))
            {
                Debug.Log($"Velocity on collision is {actor.Rb.velocity.magnitude}");
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
                    Vector3 mirroredVelocity = Vector3.Reflect(actor.Rb.velocity, collision.GetContact(0).normal);
                    var r = collision.relativeVelocity - 2 * Vector3.Dot(actor.Rb.velocity, collision.GetContact(0).normal) * collision.contacts[0].normal;

                    // Replace current velocity with the mirrored velocity
                    actor.Rb.velocity = 2 * r / 3;
                }
            }
        }
    }
}