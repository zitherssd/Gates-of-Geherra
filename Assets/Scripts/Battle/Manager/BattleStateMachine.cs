using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Battle.Manager.States;
using Assets.Scripts.Battle.Manager;

namespace Assets.Scripts.Battle
{
    public class BattleStateMachine : StateMachine
    {
        [SerializeField]
        public BattleStartState startState;
        public BattleActiveState activeState;
        public BattleEndState endState;

        public BattleStateMachine(BattleManager manager)
        {
            this.startState = new BattleStartState(manager);
            this.activeState = new BattleActiveState(manager);
            this.endState = new BattleEndState(manager);
        }     

}
}
