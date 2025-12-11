using System;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Reactions;
using Assets.Scripts.Pattern;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class BlockState : IState
    {
        private readonly Actor owner;

        [Range(0, 2)] public float DamageModifier = 1f;
        [Range(0, 2)] public float PostureModifier = 1f;
        [Range(0, 2)] public float KnockbackModifier = 1f;
        private float duration;
        private float timeSpentInBlockDuration = 0f;
        private Block skill;
        private int numberOfHits = 0;

        public BlockState(Actor owner)
        {
            this.owner = owner;
        }

        public BlockState Set(Block block)
        {
            if(block.blockType == Block.BlockType.Block)
            {
                timeSpentInBlockDuration = 0f;
                numberOfHits = 0;
            }
            this.DamageModifier = block.DamageModifier;
            this.PostureModifier = block.PostureModifier;
            this.KnockbackModifier = block.KnockbackModifier;
            this.duration = block.duration;
            this.skill = block;
            return this;
        }

        public void Enter()
        {
            if(skill.blockType == Block.BlockType.Parry)
            {
                owner.DamageApplied += OnParrySuccess;
            }
            else if (skill.blockType == Block.BlockType.Block)
            {
                owner.DamageApplied += IncreaseDuration;
                timeSpentInBlockDuration = 0f;
            }
            else if (skill.blockType == Block.BlockType.Guard)
            {
                owner.DamageApplied += GainSlowdownMeter;
            }
            owner.GetComponentInChildren<ActorUIController>().SetCC(duration, "Block");
            owner.PlayAnimation("Block");
            owner.DamageRecieved += ModifyDamage;
            owner.KnockbackRecieved += ModifyKnockback;
            owner.PostureRecieved += ModifyPosture;
           
            owner.StaminaRegenRate = 0.5f;
        }

        private void OnParrySuccess(float obj)
        {
            if (skill != null)
            {
                GainSlowdownMeter(skill.SlowdownMeterGain);
                owner.ActorData.ChangeBuildup(skill.BuildupGainOnBlock);
                skill.EndAction(ActionEndReason.Completed);
            }
        }

        private void IncreaseDuration(float obj)
        {
            if (skill == null) return;
            switch (numberOfHits)
            {
                case 0:
                    duration = Mathf.Min(1, duration + 0.5f);
                    break;
                case 1:
                    duration = Mathf.Min(1, duration + 0.33f);
                    break;
                default:
                    duration = Mathf.Min(1, duration + 0.25f);
                    break;
            }
            owner.GetComponentInChildren<ActorUIController>().SetCC(duration, "Block");
            owner.ActorData.ChangeBuildup(skill.BuildupGainOnBlock);
            numberOfHits++;
        }

        public void Exit()
        {
            if (skill != null)
            {
                if (skill.blockType == Block.BlockType.Parry)
                {
                    owner.DamageApplied -= OnParrySuccess;
                }
                else if (skill.blockType == Block.BlockType.Block)
                {
                    owner.DamageApplied -= IncreaseDuration;
                }
                else if (skill.blockType == Block.BlockType.Guard)
                {
                    owner.DamageApplied -= GainSlowdownMeter;
                }

                owner.GetComponentInChildren<ActorUIController>().HideCC();
                owner.DamageRecieved -= ModifyDamage;
                owner.KnockbackRecieved -= ModifyKnockback;
                owner.PostureRecieved -= ModifyPosture;
                owner.PlayAnimation("Idle");
                owner.StaminaRegenRate = 1f;

                // If the action is still running when we exit, it means it was interrupted.
                skill.EndAction(ActionEndReason.Interrupted);
                skill = null; // Clean up for next use.
            }
        }


        private void GainSlowdownMeter(float damage)
        {
            if (skill == null) return;
            if(owner.isControllable)
                UIManager.instance.GainMeter(skill.SlowdownMeterGain);
        }

        public float ModifyDamage(float damage)
        {
            if (skill == null) return damage;
            skill.onSucessfulBlock?.Invoke();
            float modifiedDamage = damage * DamageModifier;
            owner.ActorData.DealStaminaDamage((modifiedDamage) * skill.StaminaCostMult);
            return modifiedDamage;
        }

        public float ModifyPosture(float damage)
        {
            return damage * PostureModifier;
        }

        public float ModifyKnockback(float knockback, Vector3 direction)
        {
            return knockback * KnockbackModifier;
        }

        public void Update()
        {
            if (skill == null) return;
            if(skill.blockType == Block.BlockType.Block)
            {
                timeSpentInBlockDuration += Time.deltaTime;
                if (timeSpentInBlockDuration >= 2.5f)
                {
                    skill.EndAction(ActionEndReason.Completed);
                    return;
                }
            }
            duration -= Time.deltaTime;
            if (duration < 0)
            {
                skill.EndAction(ActionEndReason.Completed);
                return;
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            //throw new NotImplementedException();
        }
    }
}