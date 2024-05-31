using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.Effects
{
    public static class EffectsRepository
    {
        public static GameObject DamagePopupPrefab = Resources.Load<GameObject>("HpPopup");
        public static GameObject PosturePopupPrefab = Resources.Load<GameObject>("PosturePopup");
    }
}