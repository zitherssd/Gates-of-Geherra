using Assets.Scripts.Battle.Actions.Effects;
using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Core;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle.Manager.States
{
    public class BattleStartState : IState
    {
        private BattleManager manager;
        public event Action OnNewBattle;

        public BattleStartState(BattleManager manager)
        {
            this.manager = manager;
        }
        public void Enter()
        {
            foreach (var enemy in manager.EnemyActors)
            {
                enemy.state.TransitionTo<IdleState>();
            }
            UIManager.instance.EnableUI();
            UIManager.instance.EnableBattleSkills();
            OnNewBattle?.Invoke();
            ItemEventBus.Raise(ItemTrigger.OnNewBattle);
            UIManager.instance.ShowUI();

            //Transition to active
            manager.battleStateMachine.TransitionTo(manager.battleStateMachine.activeState);

        }

        public void Exit()
        {

        }

        public void OnCollisionEnter(Collision collision)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
        }
    }
}
