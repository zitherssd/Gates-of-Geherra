using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class DamagePopup : MonoBehaviour
    {
        private TextMeshPro damageText;

        public void Initialize(float damageAmount)
        {
            damageText = GetComponentInChildren<TextMeshPro>();
            damageText.text = damageAmount.ToString("F1");
            damageText.rectTransform.localScale = Vector3.zero;
            LeanTween.scale(damageText.rectTransform, Vector3.one, 0.4f).setEaseOutBack().setIgnoreTimeScale(true).setOnComplete(() =>
            {
                LeanTween.scale(damageText.rectTransform, Vector3.zero, 1f).setEaseInSine().setIgnoreTimeScale(true).setOnComplete(() => Destroy(gameObject.transform.parent.gameObject));
            });
        }
    }
}