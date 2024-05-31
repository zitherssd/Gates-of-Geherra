using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State
{
    public class AirStaggerState : IState
    {
        private Battle.Actor actor;

        public AirStaggerState(Battle.Actor actor)
        {
            this.actor = actor;
        }

        public void Enter()
        {
            actor.PlayAnimation("HurtAir");
        }

        public void Exit()
        {
        }

        public void Update()
        {
            if (actor.grounded)
            {
                actor.PlayAnimation("Down");
                actor.ActorStateMachine.TransitionTo(actor.ActorStateMachine.idleState);
            }

            //transition to landing > idle
        }
    }
}