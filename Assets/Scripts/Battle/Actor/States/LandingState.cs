using Assets.Scripts.Pattern;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class LandingState : IState
    {
        private Actor actor;

        public LandingState(Actor actor)
        {
            this.actor = actor;
        }
        public void Enter()
        {
            actor.PlayAnimation("Landing");
        }

        public void Exit()
        {
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Level"))
            {
                {
                    if (collision.gameObject.name == "LandingTrap")
                    {
                        Debug.Log($"{actor.ActorData.name} hit trap!");
                        actor.ApplyDamage(10f);
                        actor.ApplyPosture(5f);
                    }
                }
            }
        }

        public void Update()
        {
            if (actor.Rb.velocity.sqrMagnitude < Mathf.Epsilon)
            {
                actor.PlayAnimation("Idle");
                actor.state.TransitionToIdle();
            }
        }
    }
}