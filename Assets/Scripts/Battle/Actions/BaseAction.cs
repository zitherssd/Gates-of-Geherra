using System;
using UnityEngine;

namespace Assets
{
    [CreateAssetMenu(fileName = "Action", menuName = "ScriptableObjects/Action", order = 1)]

    public class BaseAction : ScriptableObject
    {
        public string Name;
        public RARITY Rarity;
        public int cooldownTurns;
        public int TotalUses;
        //public int Range;
        //public SpriteRenderer sprite;
        public int remainingUses;
        public int Speed;
        [HideInInspector] public int currentCooldownTurns = 0;

        public virtual void UpdateCooldown()
        {
            if (currentCooldownTurns > 0)
                currentCooldownTurns--;
        }

        public bool IsSkillOnCooldown()
        {
            return currentCooldownTurns > 0;
        }

        public void ResetCooldown()
        {
            currentCooldownTurns = cooldownTurns;
        }
        
        public virtual bool HasUsesLeft()
        {
            if (TotalUses == 0) return true;
            if (remainingUses > 0) return true;
            else return false;
        }

        public virtual bool IsValid(BaseActorBattler caster, out string InvalidReason)
        {
            InvalidReason = "";
            if (IsSkillOnCooldown())
            {
                InvalidReason = $"usable in {currentCooldownTurns}";
                return false;
            }
            return true;
        }

        public virtual bool IsValid(BaseActorBattler caster)
        {
            return true;
        }

        public void UpdateReaminingUses()
        {
            if (TotalUses != 0)
                remainingUses -= 1;
        }
    }






    public enum RARITY { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY };
}