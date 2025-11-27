using Assets.Scripts.Crawler;
using Assets.Scripts.Game;
using Assets.Scripts.Pattern;
using Assets.Scripts.Save;
using Assets.Scripts.Utility;
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
            SoundManager.instance.FadeOutMusic();
            UIManager.instance.Fade(true, () =>
            {
                UIManager.instance.HideUI();
                SkillGenerator.instance.DrawSkillsFromSelectionAndWaitForSelection(SkillGenerator.instance.GetRandomActions(FloorManager.instance.currentFloor), () =>
                {
                    UIManager.instance.DisableBattleSkills();

                    if (SaveManager.instance != null)
                    {
                        SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
                    }
                    GameFlowManager.instance.SetMode(GameMode.RestArea);
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
