using Assets.Scripts.Pattern;
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
            actor.ActorData.currentPosture = actor.ActorData.maxPosture;
            elapsedTime = 0f; // Reset the timer when entering the state
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
        }

        public void OnCollisionEnter(Collision collision)
        {

        }

        public void Update()
        {
            elapsedTime += Time.deltaTime; // Increment the elapsed time by the time since the last frame

            if (elapsedTime > 1.2f)
            {
                actor.ActorData.currentPosture = actor.ActorData.maxPosture;
                actor.PlayAnimation("Idle"); // Play the "GetUp" animation
                actor.state.TransitionTo(actor.state.idleState);

            }
            else if (elapsedTime > 0.6f)
            {
                actor.PlayAnimation("Getup"); // Play the "GetUp" animation

            }
        }
    }
}