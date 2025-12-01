using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Items;
using Assets.Scripts.Battle.Items.UI;
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
        private BattleManager manager;
        public BattleEndState(BattleManager manager)
        {
            this.manager = manager;
        }
        public void Enter()
        {
            SoundManager.instance.FadeOutMusic();
            UIManager.instance.Fade(true, () =>
            {
                UIManager.instance.HideUI();
                var bd = BattleManager.instance.GetCurrentBattleDefinition();
                foreach (var rewardItemPool in bd.RewardItemsPools)
                {
                    if (rewardItemPool.RewardItems != null && rewardItemPool.RewardItems.Count > 0)
                    {

                        int randomIndex = UnityEngine.Random.Range(0, rewardItemPool.RewardItems.Count);
                        BaseItem randomItem = rewardItemPool.RewardItems[randomIndex];

                        // Get random item from RewardItems list using bd.ChanceToReward
                        if (UnityEngine.Random.Range(0f, 1f) <= rewardItemPool.ChanceToReward)
                        {
                            // Add using ActorInventory
                            GameFlowManager.instance.playerActor.inventory.AddItem(randomItem);

                            // Optional: Show some UI feedback that item was awarded

                            TooltipUI.instance.ShowPrompt($"Gained <color=red>{randomItem.ItemName}</color>!");
                            Debug.Log($"Awarded item: {randomItem.ItemName}");
                        }
                    }
                }
                if (bd.RewardPool != null && bd.RewardPool.Actions.Count > 2)
                {
                    SkillGenerator.instance.DrawSkillsFromSelectionAndWaitForSelection(Get3RandomFromRewardPool(bd.RewardPool.Actions), () =>
                    {
                        UIManager.instance.DisableBattleSkills();

                        if (SaveManager.instance != null)
                        {
                            SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
                        }
                        manager.onBattleEnd?.Invoke();
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
                    manager.onBattleEnd?.Invoke();
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
