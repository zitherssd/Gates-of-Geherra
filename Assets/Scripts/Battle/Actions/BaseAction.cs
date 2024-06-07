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
        private bool free { get; set; }
        public int cooldownTurns;
        public int TotalUses;
        //public int Range;
        //public SpriteRenderer sprite;
        public int BuildupCost;
        public int BuildupGain;
        public int StaminaCost;
        public int Speed;
        public List<TAG> Tags;
        [HideInInspector] public float SliderValue;
        [HideInInspector] public Vector2 StickValue;
        public int currentCooldownTurns = 0;
        [HideInInspector] public int remainingUses;
        public float StickMult = 1;


        public virtual void Perform(Actor casterActor, Action onPerformEnd)
        {
            UpdateRemainingUses();
            ResetCooldown();

            if (!Tags.Contains(TAG.USESLIDER)) SliderValue = 1f;
            if (Tags.Contains(TAG.KILLMOMENTUM)) casterActor.GetComponent<Rigidbody>().velocity = Vector3.zero;

            casterActor.ActorData.DealStaminaDamage(StaminaCost);
            casterActor.ActorData.ChangeBuildup(BuildupGain);
            casterActor.ActorData.ChangeBuildup(-BuildupCost);

            casterActor.state.TransitionTo(casterActor.state.actingState);

            if (Tags.Contains(TAG.REPEAT_TURN))
            {
                PerformSpecific(casterActor, () => { casterActor.Act(() => { casterActor.state.TransitionTo(casterActor.state.idleState); onPerformEnd.Invoke(); }); });
            }
            else
                PerformSpecific(casterActor, () => { casterActor.state.TransitionTo(casterActor.state.idleState); onPerformEnd.Invoke(); }); 
        }

        protected virtual void PerformSpecific(Actor casterActor, Action onPerformEnd) { }

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
            if (Tags.Contains(TAG.RECHARGE_USES))
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
                InvalidReason = $"usable in {currentCooldownTurns}";
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
            if (remainingUses == TotalUses && Tags.Contains(TAG.RECHARGE_USES))
            {
                remainingUses -= 1;
                currentCooldownTurns = cooldownTurns;
            }
            else
            if (TotalUses != 0)
                remainingUses -= 1;
        }
    }


    public enum TAG
    {
        MOVE_NEAR_ENEMY_BEFORE_ATTACK, PROJECTILE, KNOCKBACK_AIR, KNOCKBACK_FRONT, KNOCKBACK_BACK, MOVE_OFFSET_BEHIND, MOVE_OFFSET_INFRONT, NO_REACTION, REPEAT_TURN, STARTER, FINISHER, COUNTER, USESLIDER, USESTICK, RECHARGE_USES,
        APPLYROOTMOTION,
        KILLMOMENTUM
    }


    public enum RARITY { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY };
}