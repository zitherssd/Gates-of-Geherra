using UnityEngine;

namespace Assets.Scripts.Battle.Components.Effects
{
    public static class EffectsRepository
    {
        public static GameObject DamagePopupPrefab = Resources.Load<GameObject>("Prefabs/HpPopup");
        public static GameObject PosturePopupPrefab = Resources.Load<GameObject>("Prefabs/PosturePopup");
    }
}