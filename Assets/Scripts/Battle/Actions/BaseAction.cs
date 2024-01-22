using Assets.Scripts.Actions;
using System;
using System.Collections.Generic;
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
        public int BuildupCost;
        public int BuildupGain;
        public int Speed;
        public List<TAG> Tags;
        public float SliderValue;
        public Vector2 StickValue;
        [HideInInspector] public int currentCooldownTurns = 0;

        public virtual void Perform(BaseActorBattler casterActor, Action onPerformEnd)
        {

        }

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

        public virtual bool IsValid()
        {
            return true;
        }

        public virtual bool IsValidAndInRange(BaseActorBattler caster)
        {
            return true;
        }

        public void UpdateReaminingUses()
        {
            if (TotalUses != 0)
                remainingUses -= 1;
        }
    }




    public enum TAG { MOVE_NEAR_ENEMY_BEFORE_ATTACK, PROJECTILE, KNOCKBACK_AIR, KNOCKBACK_BACK, KNOCKBACK_FRONT, MOVE_OFFSET_BEHIND, MOVE_OFFSET_INFRONT, NO_REACTION, REPEAT_TURN, STARTER, FINISHER, COUNTER, USESLIDER, USEKNOB,
        APPLYROOTMOTION
    }

    public enum RARITY { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY };
}