using Assets.Scripts.Crawler;
using Assets.Scripts.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle.Manager.States
{
    public class BattleEndState : IState
    {
        public BattleEndState(BattleManager manager)
        {
                
        }
        public void Enter()
        {
            UIManager.instance.Fade(true, () =>
            {
                SkillGenerator.instance.DrawSkillsFromSelectionAndWaitForSelection(SkillGenerator.instance.GetRandomActions(FloorManager.instance.currentFloor), () =>
                {
                    UIManager.instance.ShowRestingUI();
                });
            });
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
