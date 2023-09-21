using System;
using UnityEngine;

namespace Assets
{
    [CreateAssetMenu(fileName = "Action", menuName = "ScriptableObjects/Action", order = 1)]

    public class BaseAction : ScriptableObject
    {
        public string Name;
        public RARITY Rarity;
        public int TotalUses;
        //public int Range;
        //public SpriteRenderer sprite;
        public int remainingUses;

        public virtual bool HasUsesLeft()
        {
            if (TotalUses == 0) return true;
            if (remainingUses > 0) return true;
            else return false;
        }

        public virtual bool IsValid(BaseActorBattler caster, out string InvalidReason)
        {
            InvalidReason = "";
            return true;
        }

        public virtual bool IsValid(BaseActorBattler caster)
        {
            return true;
        }
    }






    public enum RARITY { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY };
}