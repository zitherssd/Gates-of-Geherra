using Assets.Scripts.Pattern;
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
            UIManager.instance.EnableUI();
            OnNewBattle?.Invoke();

            //Gain meter
            UIManager.instance.GainMeter(4f);
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
