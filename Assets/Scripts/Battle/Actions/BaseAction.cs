using Assets.Scripts.Battle;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class BaseAction : ScriptableObject
    {
        public string Name;
        public RARITY Rarity;
        public string Description;
        public float CooldownTimer;
        public int TotalUses;
        //public int Range;
        //public SpriteRenderer sprite;
        public int BuildupCost;
        public int BuildupGain;
        public int StaminaCost;
        public int Speed;
        public List<TAG> Tags;
        [HideInInspector] public Vector3 Direction;
        public float currentCooldownTimer = 0;
        [HideInInspector] public int remainingUses;
        public float StickMult = 1;


        public virtual void Perform(Actor casterActor, Action onPerformEnd)
        {
            UpdateRemainingUses();
            //ResetCooldown();

            if (Tags.Contains(TAG.KILLMOMENTUM)) casterActor.GetComponent<Rigidbody>().velocity = Vector3.zero;

            casterActor.ActorData.DealStaminaDamage(StaminaCost);
            casterActor.ActorData.ChangeBuildup(BuildupGain);
            casterActor.ActorData.ChangeBuildup(-BuildupCost);

            PerformSpecific(casterActor, onPerformEnd);
        }

        protected virtual void PerformSpecific(Actor casterActor, Action onPerformEnd) { }

        public virtual void UpdateCooldown()
        {
            if (currentCooldownTimer > 0)
                currentCooldownTimer = currentCooldownTimer -= Time.deltaTime;
            else
                currentCooldownTimer = 0f;

            if (Tags.Contains(TAG.RECHARGE_TOTAL_USES) && remainingUses < TotalUses)
            {
                if (currentCooldownTimer == 0)
                {
                    remainingUses++;
                    currentCooldownTimer = CooldownTimer;
                }
            }
        }

        public bool IsSkillOnCooldown()
        {
            if (Tags.Contains(TAG.RECHARGE_TOTAL_USES)) return false;
            return currentCooldownTimer > 0;
        }

        public void ResetCooldown()
        {
            currentCooldownTimer = CooldownTimer;
            if (Tags.Contains(TAG.RECHARGE_TOTAL_USES))
            {
                if(remainingUses < TotalUses)
                {
                    remainingUses++;
                    currentCooldownTimer = CooldownTimer;
                }    
            }
            else
                currentCooldownTimer = CooldownTimer;

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
            currentCooldownTimer = 0;
        }

        public virtual bool IsValid(Actor caster, out string InvalidReason)
        {
            InvalidReason = "";

            if (!HasUsesLeft())
            {
                InvalidReason = "No uses left!";
                return false;
            }
            if (IsSkillOnCooldown())
            {
                InvalidReason = $"usable in {currentCooldownTimer}";
                return false;
            }
            if (caster.ActorData.currentBuildup < BuildupCost)
            {
                InvalidReason = "Not enough Buildup!";
                return false;
            }
            if (caster.ActorData.currentStamina < StaminaCost)
            {
                InvalidReason = "Not enough Stamina!";
                return false;
            }
            return true;
        }

        public virtual bool IsValidAndInRange(Actor caster)
        {
            return false;
        }

        public void UpdateRemainingUses()
        {
            currentCooldownTimer = CooldownTimer;

            if (remainingUses == TotalUses && Tags.Contains(TAG.RECHARGE_TOTAL_USES))
            {
                remainingUses -= 1;
                currentCooldownTimer = CooldownTimer;
            }
            else
            if (remainingUses != 0)
                remainingUses -= 1;
        }

        public enum TAG
        {
            PROJECTILE, KNOCKBACK_AIR, KNOCKBACK_FRONT, KNOCKBACK_BACK, NO_REACTION, FREE, STARTER, FINISHER, COUNTER, USESTICK, RECHARGE_TOTAL_USES,
            APPLYROOTMOTION,
            KILLMOMENTUM, KILL_TRACKING
        }
    }





    public enum RARITY { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY };
    public enum ANIMATION { NONE, Punch, Kick, Shuriken, Highkick, PalmStrike, Ninjutsu, ForwardPunch, ThrowStar, ForwardKick, ShadowStep, Taunt, Dash, Roll, Step }

}