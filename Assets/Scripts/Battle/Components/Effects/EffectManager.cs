using Assets.Scripts.Utility;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.Effects
{
    public class EffectManager
    {
        private readonly Actor owner;
        public EffectManager(Actor owner)
        {
            this.owner = owner;

            owner.OnDamageApplied += ShowDamagePopup;
            owner.OnPostureApplied += ShowPosturePopup;
            owner.OnDamageApplied += FlashWhite;
        }

        private void ShowDamagePopup(float damageAmount)
        {
            // Instantiate the damage popup prefab
            GameObject popup = GameObject.Instantiate(EffectsRepository.DamagePopupPrefab, owner.transform.position, Quaternion.identity);

            popup.GetComponentInChildren<DamagePopup>().Initialize(damageAmount);

        }
        private void ShowPosturePopup(float damageAmount)
        {
            // Instantiate the damage popup prefab
            GameObject popup = GameObject.Instantiate(EffectsRepository.PosturePopupPrefab, owner.transform.position, Quaternion.identity);

            // Set the damage amount text
            popup.GetComponentInChildren<DamagePopup>().Initialize(damageAmount);
        }

        private void FlashWhite(float damageAmount)
        {
            var cc = owner.GetComponentInChildren<ColorController>();

            var intensity = (damageAmount / owner.ActorData.maxPosture);
            cc.StartCoroutine(cc.FlashWhite(0.3f, Mathf.Clamp(intensity,0,1.2f)));
        }
        
    }
}