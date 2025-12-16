using System;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Pattern;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class StaggerState : IState
    {
        public event Action<float> OnStaggerStateEntered;
        public event Action OnStaggerStateExit;

        private Actor actor;
        private float timer = 0f;
        private bool collisionOccured;
        private float duration = 0f;



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


        public void Enter()
        {
            if (actor.isControllable)
            {
                UIManager.GetInstance().HideUI();
                var Ready = false;
            }

            actor.PlayAnimation("PostureBroken");
            OnStaggerStateEntered?.Invoke(duration);
            actor.audio.PlayAudio("Attack1");
            actor.movement.SetFriction(0.4f);
            collisionOccured = false;
            timer = 0f;
        }

        public void Exit()
        {
            OnStaggerStateExit?.Invoke();
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

                collisionOccured = true;

                // Check if velocity magnitude is greater than the threshold
                if (collision.relativeVelocity.magnitude > 0.1f)
                {
                    Debug.Log(collision.relativeVelocity.magnitude);
                    var damageInstance = new DamageInstance
                    {
                        Damage = 2f,
                        PostureDamage = 5f,
                    };
                    actor.ApplyDamageInstance(damageInstance, actor, null);
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