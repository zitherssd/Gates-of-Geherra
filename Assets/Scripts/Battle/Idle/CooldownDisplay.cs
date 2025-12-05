using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.Battle.Idle
{
    public class CooldownDisplay : MonoBehaviour
    {
        public string timeLockId;               // e.g. "QuickFightCooldown"
        public Button targetButton;             // the Quick Fight button
        public TMP_Text countdownText;          // UI text showing mm:ss

        private TimeLock currentLock;
        private bool isVisible = false;         // track scaling state

        private void Start()
        {
            transform.localScale = Vector3.zero; // start hidden
            RefreshLock();
        }

        private void Update()
        {
            if (currentLock == null)
                return;

            if (currentLock.IsDone)
            {

                targetButton.interactable = true;
                countdownText.text = "";

                currentLock = null;

                HideUI();
                return;
            }

            // Update UI text
            var rem = currentLock.Remaining;
            countdownText.text = $"{rem.Minutes:D2}:{rem.Seconds:D2}";
        }

        private void OnEnable() => RefreshLock();

        public void RefreshLock()
        {
            currentLock = TimeLockManager.Get(timeLockId);

            if (currentLock != null)
            {
                // Cooldown active
                targetButton.interactable = false;
                ShowUI();
            }
            else
            {
                // No cooldown
                targetButton.interactable = true;
                HideUI();
            }
        }

        private void ShowUI()
        {
            if (isVisible) return;
            isVisible = true;

            LeanTween.scale(gameObject, Vector3.one, 0.25f).setEaseOutBack();
        }

        private void HideUI()
        {
            if (!isVisible) return;
            isVisible = false;

            LeanTween.scale(gameObject, Vector3.zero, 0.2f).setEaseInBack();
        }
    }
}
