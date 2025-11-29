using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Crawler;
using Assets.Scripts.Game;
using Assets.Scripts.Pattern;
using Assets.Scripts.Save;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
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
                var bd = BattleManager.instance.GetCurrentBattleDefinition();
                if (bd.RewardPool != null && bd.RewardPool.Actions.Count > 2)
                {
                    SkillGenerator.instance.DrawSkillsFromSelectionAndWaitForSelection(Get3RandomFromRewardPool(bd.RewardPool.Actions), () =>
                    {
                        UIManager.instance.DisableBattleSkills();

                        if (SaveManager.instance != null)
                        {
                            SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
                        }
                        GameFlowManager.instance.SetMode(GameMode.RestArea);
                    });
                }
                else
                {
                    UIManager.instance.DisableBattleSkills();

                    if (SaveManager.instance != null)
                    {
                        SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
                    }
                    GameFlowManager.instance.SetMode(GameMode.RestArea);
                }
            });
        }

        private BaseAction[] Get3RandomFromRewardPool(List<BaseAction> actions)
        {
            var selected = actions
            .OrderBy(a => UnityEngine.Random.value)
            .Take(Mathf.Min(3, actions.Count));

            // Return clones instead of references
            return selected
                .Select(a => ScriptableObject.Instantiate(a))
                .ToArray();
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
