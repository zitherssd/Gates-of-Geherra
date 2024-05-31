using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State
{
    public class StaggerState : IState
    {
        private Battle.Actor actor;
        private float idleTimer = 0f;

        public StaggerState(Battle.Actor actor)
        {
            this.actor = actor;
        }

        public void Enter()
        {
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
            actor.KnockbackRecieved -= ModifyKnockback;
            actor.DamageApplied -= ChangeSpriteToDamaged;
        }

        public void Update()
        {
            if (!actor.grounded)
                actor.ActorStateMachine.TransitionTo(actor.ActorStateMachine.airStaggerState);
            if (actor.rigidbody.velocity.magnitude > Mathf.Epsilon)
            {
                idleTimer = 0f;
            }
            else
            {
                // Increment the idle timer if the actor's velocity is zero
                idleTimer += Time.deltaTime;

                // Transition to idleState if the idle timer exceeds 1 second
                if (idleTimer > 1f)
                {
                    actor.ActorStateMachine.TransitionTo(actor.ActorStateMachine.idleState);
                }
            }
        }
    }
}