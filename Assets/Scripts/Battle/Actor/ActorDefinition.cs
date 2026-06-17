using System.Collections.Generic;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor.AI;
using Assets.Scripts.Battle.Items;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Actor", menuName = "ScriptableObjects/Actor", order = 1)]
    public class ActorDefinition : ScriptableObject
    {
        public string guid;
        public string Name;
        public List<HpBar> hpBars;

        public float baseMaxBuildup;
        public float baseMaxPosture;
        public float baseMaxStamina;

        public int Strength;
        public int Agility;
        public int Mind;
        public int Spirit;

        public float postureRegenRate = 1f;
        public float staminaRegenRate = 1f;

        public bool Controllable;
        public AiRuleset AIRuleset;

        public Color mainColor;
        public Color secondaryColor;

        public List<BaseItem> startingItems;
        public List<BaseAction> baseActions;
    }
}



