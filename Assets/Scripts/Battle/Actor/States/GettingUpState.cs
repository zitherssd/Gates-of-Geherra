using Assets.Scripts.Battle.Status;
using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class GettingUpState : IState
    {
        private Actor actor;
        private CapsuleCollider cc;
        private float elapsedTime;

        public void Enter()
        {
            cc.height = 0.4f;
            cc.center = new Vector3(0, 0.3f, 0);
            actor.Runtime.currentPosture = actor.Runtime.maxPosture;
            elapsedTime = 0f; // Reset the timer when entering the state

            // Full immunity (damage, posture, knockback) while down and getting up.
            if (!actor.statusManager.HasStatus<InvincibilityStatus>())
                actor.statusManager.Add(ScriptableObject.CreateInstance<InvincibilityStatus>());
        }

        public GettingUpState(Actor actor)
        {
            this.actor = actor;
            cc = actor.GetComponent<CapsuleCollider>();
        }

        public void Exit()
        {
            cc.height = 1.2f;
            cc.center = new Vector3(0, 0.6f, 0);

            // Remove invincibility now that the actor is back on their feet.
            var active = actor.statusManager.GetStatus<InvincibilityStatus>();
            if (active != null)
                actor.statusManager.Remove(active);
        }

        public void OnCollisionEnter(Collision collision)
        {

        }

        public void Update()
        {
            elapsedTime += Time.deltaTime; // Increment the elapsed time by the time since the last frame

            if (actor.Runtime.isDead())
            {
                actor.state.TransitionTo<DeathState>();
                return;
            }

            if (elapsedTime > 1.2f)
            {
                actor.Runtime.currentPosture = actor.Runtime.maxPosture;
                actor.PlayAnimation("Idle"); // Play the "GetUp" animation
                actor.state.TransitionTo<IdleState>();

            }
            else if (elapsedTime > 0.6f)
            {
                actor.PlayAnimation("Getup"); // Play the "GetUp" animation

            }
        }
    }
}