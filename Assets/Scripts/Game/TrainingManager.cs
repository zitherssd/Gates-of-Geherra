using Assets.Scripts.Battle.Items.UI;
using Assets.Scripts.Save;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Game
{
    public class TrainingManager : MonoBehaviour
    {
        public Button trainingButton;
        public Button quickFightButton;
        public static TrainingManager instance;
        private TimeLock trainingLock;
        private bool visible;


        private void Awake()
        {
            instance = this;
        }

        public void Enter()
        {
            GameFlowManager.instance.playerActor.PlayAnimation("Fire");
            Refresh();
        }

        void Update()
        {
            Refresh();

            if (trainingLock == null)
            {
                return;

            }

            if (trainingLock.IsDone)
            {
                AwardRandomStat();
                TimeLockManager.Remove("Training");
                trainingLock = null;
                trainingButton.interactable = true;
                return;
            }

        }

        public void TriggerTraining()
        {
            TimeSpan timespan = TimeSpan.Zero;
            GameFlowManager.instance.trainingsDone++;
            var trainingsDone = GameFlowManager.instance.trainingsDoneThisFloor;
            switch (trainingsDone)
            {
                case 0:
                    timespan = TimeSpan.FromSeconds(10);
                    break;
                case 1:
                    timespan = TimeSpan.FromMinutes(1);
                    break;
                case 2:
                    timespan = TimeSpan.FromMinutes(3);
                    break;
                case 3:
                    timespan = TimeSpan.FromMinutes(5);
                    break;
                case 4:
                    timespan = TimeSpan.FromMinutes(10);
                    break;
                case >4:
                    timespan = TimeSpan.FromMinutes(15);
                    break;
            }

            TimeLockManager.Add("Training", timespan);
            GameFlowManager.instance.trainingsDoneThisFloor++;
            Refresh();
        }

        void OnEnable() => Refresh();

        public void Refresh()
        {
            trainingLock = TimeLockManager.Get("Training");
            var quickFightLock = TimeLockManager.Get("QuickFight");

            if(trainingLock != null || quickFightLock != null)
            {
                quickFightButton.interactable = false;
            }
            else
            {
                quickFightButton.interactable = true;
            }

            if (trainingLock != null)
            {
                trainingButton.interactable = false;
                Show();
            }
            else
            {
                trainingButton.interactable = true;
                Hide();
            }
        }

        private void Show()
        {
            if (visible) return;
            visible = true;
        }

        private void Hide()
        {
            if (!visible) return;
            visible = false;
            LeanTween.scale(gameObject, Vector3.zero, 0.2f).setEaseInBack();
        }

        private void AwardRandomStat()
        {
            var ad = GameFlowManager.instance.playerActor.ActorData;

            // Pick a random stat index
            int roll = UnityEngine.Random.Range(0, 6);

            switch (roll)
            {
                case 0:
                    ad.ATK += 1;
                    TooltipUI.instance.ShowPrompt("As a result of your training, you gain +1 STR");
                    Debug.Log("Training Reward: +1 Strength");
                    break;
                case 1:
                    ad.DEF += 1;
                    TooltipUI.instance.ShowPrompt("As a result of your training, you gain +1 DEF");
                    Debug.Log("Training Reward: +1 Body/Constitution");
                    break;
                case 2:
                    ad.AGI += 1;
                    TooltipUI.instance.ShowPrompt("As a result of your training, you gain +1 AGI");
                    Debug.Log("Training Reward: +1 Agility");
                    break;
                case 3:
                    ad.Spirit += 1;
                    TooltipUI.instance.ShowPrompt("As a result of your training, you gain +1 Spirit");
                    Debug.Log("Training Reward: +1 Spirit");
                    break;
                case 4:
                    ad.maxStamina += 3;
                    TooltipUI.instance.ShowPrompt("As a result of your training, you gain +3 Max Stamina");
                    break;
                case 5:
                    ad.maxBuildup += 3;
                    TooltipUI.instance.ShowPrompt("As a result of your training, you gain +3 Max Buildup");
                    break;
                case 6:
                    ad.maxPosture += 3;
                    TooltipUI.instance.ShowPrompt("As a result of your training, you gain +3 Max Posture");
                    break;
                    
            }

            // Optional: Save automatically
            SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
        }

        public void Train(TimeSpan trainingEndTime)
        {
            TimeLockManager.Add("Training", trainingEndTime);
        }
    }
}
