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
        private bool visible;


        private void Awake()
        {
            instance = this;
        }

        public void Enter()
        {
            GameFlowManager.instance.playerActor.PlayAnimation("Fire");
        }

        void Update()
        {

            var trainingLock = TimeLockManager.Get("Training");
            var quickFightLock = TimeLockManager.Get("QuickFight");

            if (quickFightLock != null && quickFightLock.IsDone)
                TimeLockManager.Remove("QuickFight");

            if (trainingLock != null || quickFightLock != null)
                quickFightButton.interactable = false;
            else
                quickFightButton.interactable = true;
            

            if (trainingLock == null)
            {
                trainingButton.interactable = true;
                return;

            }
            else if (trainingLock.IsDone)
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
                    timespan = TimeSpan.FromSeconds(11);
                    break;
                case 2:
                    timespan = TimeSpan.FromSeconds(12);
                    break;
                case 3:
                    timespan = TimeSpan.FromSeconds(13);
                    break;
                case 4:
                    timespan = TimeSpan.FromSeconds(14);
                    break;
                case >4:
                    timespan = TimeSpan.FromSeconds(15);
                    break;
            }

            TimeLockManager.Add("Training", timespan);
            GameFlowManager.instance.trainingsDoneThisFloor++;
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
            int roll = UnityEngine.Random.Range(0, 3);

            switch (roll)
            {
                case 0:
                    ad.Strength += 1;
                    TooltipUI.instance.ShowPrompt("As a result of your training, you gain +1 STR");
                    break;
                case 1:
                    ad.Agility += 1;
                    TooltipUI.instance.ShowPrompt("As a result of your training, you gain +1 AGI");
                    break;
                case 2:
                    ad.Mind += 1;
                    TooltipUI.instance.ShowPrompt("As a result of your training, you gain +1 MND");
                    break;
                case 3:
                    TooltipUI.instance.ShowPrompt("Your training failed to produce any results");
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
