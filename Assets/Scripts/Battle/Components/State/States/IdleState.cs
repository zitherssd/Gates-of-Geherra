using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State
{
    public class IdleState : IState
    {
        private Battle.Actor actor;
        private SpriteRenderer spriteRenderer;

        public IdleState(Battle.Actor actor)
        {
            this.actor = actor;
            spriteRenderer = actor.GetComponent<SpriteRenderer>();
        }

        public void Enter()
        {
            //Get closest opponent
            //var opponent = actor.isControllable() ? BattleManager.instance.EnemyActors[0] : BattleManager.instance.PlayerActors[0];

        }

        public void Exit()
        {

        }

        public void Update()
        {
            if (!actor.grounded) actor.ActorStateMachine.TransitionTo(actor.ActorStateMachine.airStaggerState);
        }
    }
}