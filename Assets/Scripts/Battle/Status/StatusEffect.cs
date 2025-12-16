using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Status
{
    [CreateAssetMenu(fileName = "NewStatusEffect", menuName = "ScriptableObjects/Status/StatusEffect")]
    public class StatusEffect : ScriptableObject
    {
        [Header("Info")]
        public string Name;
        [TextArea] public string Description;
        public Sprite Icon;

        [Header("Timing")]
        public float Duration; // 0 for infinite/until removed

        [Header("Behavior")]
        public bool IsStackable;
        public int MaxStacks;

        [SerializeReference]
        public List<EffectLogic> EffectLogics = new List<EffectLogic>();
    }
}
