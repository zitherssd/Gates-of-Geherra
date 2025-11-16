using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
{
    public class BaseAction : ScriptableObject
    {
        public BUTTONTYPE Type;
        public string Name;
        public RARITY Rarity;
        public string Description;
        public float CooldownTimer;
        public int TotalUses;
        public int remainingUses;
        //public int Range;
        //public SpriteRenderer sprite;
        public int BuildupCost;
        public int BuildupGain;
        public int StaminaCost;
        public int Speed;
        public List<TAG> Tags;
        [HideInInspector] public Vector3 Direction;
        public float currentCooldownTimer = 0;
        public float StickMult = 1;
        public float SlowdownMeterGain;
        public float SlowdownMeterGainOnEnd;
        public float SlowDownMeterGainOnPress;
        public float SlowdownMeterGainOnRelease;
        public Action OnCancel;

        public enum BUTTONTYPE { INSTANT, VECTOR, CONTINNUOUS, CONTINUOUS_VECTOR };

        public virtual void Perform(Actor.Actor casterActor, Action onPerformEnd)
        {
            //ResetCooldown();

            if (Tags.Contains(TAG.KILLMOMENTUM)) casterActor.movement.ResetMomentum();

            casterActor.ActorData.DealStaminaDamage(StaminaCost);
            casterActor.ActorData.ChangeBuildup(BuildupGain);
            casterActor.ActorData.ChangeBuildup(-BuildupCost);

            PerformSpecific(casterActor, () =>
            {
                if(casterActor.isControllable) UIManager.instance.GainMeter(SlowdownMeterGainOnEnd);
                UpdateRemainingUses(); onPerformEnd?.Invoke();
            });
        }

        protected virtual void PerformSpecific(Actor.Actor casterActor, Action onPerformEnd) { }

        public virtual void UpdateCooldown() //runs every frame
        {
            if (TotalUses != 0) //limited number of uses or recharges uses
            {
                if (Tags.Contains(TAG.RECHARGE_TOTAL_USES)) //recharges uses
                {
                    if (remainingUses < TotalUses && currentCooldownTimer == 0)
                    {
                        currentCooldownTimer = CooldownTimer;
                    }
                }
            }


            if (currentCooldownTimer > 0)
            {
                if (Tags.Contains(TAG.RECHARGE_DURING_SLOWDOWN))
                    currentCooldownTimer = currentCooldownTimer -= Time.unscaledDeltaTime;
                else
                    currentCooldownTimer = currentCooldownTimer -= Time.deltaTime;

            }
            else
                currentCooldownTimer = 0f;


            if (Tags != null)
                if (TotalUses != 0) //limited number of uses or recharges uses
                {
                    if (Tags.Contains(TAG.RECHARGE_TOTAL_USES)) //recharges uses
                    {
                        if (remainingUses < TotalUses && currentCooldownTimer == 0)
                        {
                            remainingUses++;
                        }
                    }
                }
        }

        public bool IsSkillOnCooldown()
        {
            if (Tags.Contains(TAG.RECHARGE_TOTAL_USES)) return false;
            return currentCooldownTimer > 0;
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

        public virtual bool IsValid(Actor.Actor caster, out string InvalidReason)
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

        public virtual bool IsValidAndInRange(Actor.Actor caster)
        {
            return false;
        }

        public void UpdateRemainingUses() //Runs when animation ends
        {
            if (TotalUses != 0) //limited number of uses or recharges uses
            {
                remainingUses--;

                if (!Tags.Contains(TAG.RECHARGE_TOTAL_USES))
                {
                    currentCooldownTimer = CooldownTimer;
                }
                else if (remainingUses <= 0 && currentCooldownTimer == 0)
                {
                    // Start the recharge cycle only when remaining uses reach 0
                    currentCooldownTimer = CooldownTimer;
                }
            }
            else
            {
                currentCooldownTimer = CooldownTimer;
            }
        }

        public enum TAG
        {
            PROJECTILE, KNOCKBACK_AIR, KNOCKBACK_FRONT, KNOCKBACK_BACK, NO_REACTION, FREE, STARTER, FINISHER, COUNTER, USESTICK, RECHARGE_TOTAL_USES,
            APPLYROOTMOTION,
            KILLMOMENTUM, KILL_TRACKING, PLAY_WHILE_SELECTING,
            TECH, FACECLOSEST, RECHARGE_DURING_SLOWDOWN
        }

        public void OnActionEnd()
        {

        }
    }





    public enum RARITY { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY };
    public enum ANIMATION { NONE, Punch, Kick, Shuriken, Highkick, PalmStrike, Ninjutsu, ForwardPunch, ThrowStar, ForwardKick, ShadowStep, Taunt, Dash, Roll, Step, Firecast }

}