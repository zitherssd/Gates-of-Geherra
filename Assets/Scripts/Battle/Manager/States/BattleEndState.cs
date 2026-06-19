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
            ResetSlowdown();
                
            
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
                            BattleManager.instance.Player.inventory.AddItem(randomItem);

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
                        ResetSlowdown();
                        UIManager.instance.DisableBattleSkills();

                        if (SaveManager.instance != null)
                        {
                            SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
                        }
                            manager.TriggerBattleEnd();
                        ReturnAfterBattle();
                    });
                }
                else
                {
                    ResetSlowdown();
                    UIManager.instance.DisableBattleSkills();

                    if (SaveManager.instance != null)

                    {
                        SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
                    }
                        manager.TriggerBattleEnd();
                    ReturnAfterBattle();
                }
            });
        }

        private static void ResetSlowdown()
        {
            if (SlowdownManager.instance != null)
                SlowdownManager.instance.ResetImmediate();
        }

        // Arena battles return to the scene we came from; legacy in-scene battles just flip the
        // shared scene back to rest mode. Player data already lives on GameSession, so no disk
        // round-trip is needed across the scene load.
        private void ReturnAfterBattle()
        {
            ResetSlowdown();

            // Keep the persisted layout in sync with any post-battle changes (e.g. a reward skill
            // added to the inventory) so the rest scene rebuilds the same arrangement.
            if (GameSession.Exists && UIManager.instance != null)
                GameSession.Instance.Loadout = UIManager.instance.GetCurrentLoadout();

            if (GameSession.Exists && !string.IsNullOrEmpty(GameSession.Instance.ReturnScene))
            {
                var scene = GameSession.Instance.ReturnScene;
                GameSession.Instance.ReturnScene = null;
                UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
                return;
            }

            if (GameFlowManager.instance != null)
                GameFlowManager.instance.SetMode(GameMode.RestArea);
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
