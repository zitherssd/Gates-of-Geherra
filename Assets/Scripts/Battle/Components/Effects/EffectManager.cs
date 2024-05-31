using Assets.Scripts.Utility;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.Effects
{
    public class EffectManager
    {
        private Actor owner;
        public EffectManager(Actor owner)
        {
            this.owner = owner;

            owner.DamageApplied += ShowDamagePopup;
            owner.PostureApplied += ShowPosturePopup;
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
    }
}