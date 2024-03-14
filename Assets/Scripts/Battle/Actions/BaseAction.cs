using Assets.Scripts.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class BaseAction : ScriptableObject
    {
        public string Name;
        public RARITY Rarity;
        public int cooldownTurns;
        public int TotalUses;
        //public int Range;
        //public SpriteRenderer sprite;
        public int BuildupCost;
        public int BuildupGain;
        public int Speed;
        public List<TAG> Tags;
        [HideInInspector] public float SliderValue;
        [HideInInspector] public Vector2 StickValue;
        [HideInInspector] public int currentCooldownTurns = 0;
        [HideInInspector] public int remainingUses;

        public virtual void Perform(BaseActorBattler casterActor, Action onPerformEnd)
        {
            UpdateRemainingUses();
            ResetCooldown();

            if (!Tags.Contains(TAG.USESLIDER)) SliderValue = 1f;
            if (Tags.Contains(TAG.KILLMOMENTUM)) casterActor.GetComponent<Rigidbody>().velocity = Vector3.zero;

            casterActor.Actor.ChangeBuildup(BuildupGain);
            casterActor.Actor.ChangeBuildup(-BuildupCost);
            PerformSpecific(casterActor, onPerformEnd);
        }

        protected virtual void PerformSpecific(BaseActorBattler casterActor, Action onPerformEnd) { }

        public virtual void UpdateCooldown()
        {
            if (currentCooldownTurns > 0)
                currentCooldownTurns--;

            if (Tags.Contains(TAG.RECHARGE_USES) && remainingUses < TotalUses)
            {
                if (currentCooldownTurns == 0)
                {
                    remainingUses++;
                    currentCooldownTurns = cooldownTurns;
                }
            }
        }

        public bool IsSkillOnCooldown()
        {
            if (Tags.Contains(TAG.RECHARGE_USES)) return false;
            return currentCooldownTurns > 0;
        }

        public void ResetCooldown()
        {
            if(Tags.Contains(TAG.RECHARGE_USES))
                {

                }
            else

            currentCooldownTurns = cooldownTurns;

        }
        
        public virtual bool HasUsesLeft()
        {
            if (TotalUses == 0) return true;
            if (remainingUses > 0) return true;
            else return false;
        }

        internal void Refresh()
        {
            remainingUses = TotalUses;
            currentCooldownTurns = 0;
        }

        public virtual bool IsValid(BaseActorBattler caster, out string InvalidReason)
        {
            InvalidReason = "";

            if (!HasUsesLeft())
            {
                InvalidReason = "No uses left!";
                return false;
            }
            if (IsSkillOnCooldown())
            {
                InvalidReason = $"usable in {currentCooldownTurns}";
                return false;
            }
            if (caster.Actor.currentBuildup < BuildupCost)
            {
                InvalidReason = "Not enough Buildup!";
                return false;
            }
            return true;
        }

        public virtual bool IsValidAndInRange(BaseActorBattler caster)
        {
            return true;
        }

        public void UpdateRemainingUses()
        {
            if(remainingUses == TotalUses && Tags.Contains(TAG.RECHARGE_USES))
            {
                remainingUses -= 1;
                currentCooldownTurns = cooldownTurns;
            }
            else 
            if (TotalUses != 0)
                remainingUses -= 1;
        }
    }


    public enum TAG { MOVE_NEAR_ENEMY_BEFORE_ATTACK, PROJECTILE, KNOCKBACK_AIR, KNOCKBACK_FRONT, KNOCKBACK_BACK, MOVE_OFFSET_BEHIND, MOVE_OFFSET_INFRONT, NO_REACTION, REPEAT_TURN, STARTER, FINISHER, COUNTER, USESLIDER, USEKNOB, RECHARGE_USES,
        APPLYROOTMOTION,
        KILLMOMENTUM
    }


    public enum RARITY { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY };
}