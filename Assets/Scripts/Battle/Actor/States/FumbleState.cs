using Assets.Scripts.Pattern;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class FumbleState : IState
    {
        private Actor actor;
        private float timer = 0f;
        private bool collisionOccured;
        private float duration = 0f;

        public FumbleState(Actor actor)
        {
            this.actor = actor;
        }

        public FumbleState Set(float duration)
        {
            this.duration = duration;
            return this;
        }

        public void Enter()
        {
            Physics.IgnoreLayerCollision(3, 3, true);
            //actor.KnockbackRecieved += ModifyKnockback;
            actor.PlayAnimation("PostureBroken");
            actor.audio.PlayAudio("Parry");
        }

        public void Exit()
        {
            Physics.IgnoreLayerCollision(3, 3, false);

        }

        public void Update()
        {
            if (!actor.grounded)
                actor.state.TransitionTo<AirStaggerState>();
            duration -= Time.deltaTime;
            if (duration < 0)
            {
                actor.state.TransitionToIdle();
                duration = 0;
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Level"))
            {
              
            }
        }
    }
}