using System;
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
        public Action OnEnd;
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
                owner.DamageApplied += Cancel;
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

        private void Cancel(float obj)
        {
            skill.Cancel();
        }

        private void IncreaseDuration(float obj)
        {

            switch (numberOfHits)
            {
                case 0:
                    duration = Mathf.Max(1, duration + 0.5f);
                    break;
                case 1:
                    duration = Mathf.Max(1, duration + 0.33f);
                    break;
                default:
                    duration = Mathf.Max(1, duration + 0.25f);
                    break;
            }
            owner.GetComponentInChildren<ActorUIController>().SetCC(duration, "Block");
            owner.ActorData.ChangeBuildup(skill.BuildupGainOnBlock);
            numberOfHits++;
        }

        public void Exit()
        {
            if (skill.blockType == Block.BlockType.Parry)
            {
                owner.DamageApplied -= Cancel;
                owner.ActorData.ChangeBuildup(skill.BuildupGainOnBlock);
            }
            else
            {
                owner.DamageApplied -= IncreaseDuration;
            }
            owner.GetComponentInChildren<ActorUIController>().HideCC();
            owner.DamageRecieved -= ModifyDamage;
            owner.KnockbackRecieved -= ModifyKnockback;
            owner.PostureRecieved -= ModifyPosture;
            owner.PlayAnimation("Idle");
            owner.StaminaRegenRate = 1f;
            OnEnd?.Invoke();
        }


        private void GainSlowdownMeter(float damage)
        {
            if(owner.isControllable)
            UIManager.instance.GainMeter(skill.SlowdownMeterGain);
        }

        public float ModifyDamage(float damage)
        {
            skill.onSucessfulBlock?.Invoke();
            float modifiedDamage = damage * DamageModifier;
            owner.ActorData.DealStaminaDamage((modifiedDamage) * skill.StaminaCostMult);
            return modifiedDamage;
        }

        public float ModifyPosture(float damage)
        {
            float modifiedDamage = damage * PostureModifier;
            return modifiedDamage;
        }

        public float ModifyKnockback(float knockback, Vector3 direction)
        {
            return knockback * KnockbackModifier;
        }

        public void Update()
        {

            timeSpentInBlockDuration += Time.deltaTime;
            if (timeSpentInBlockDuration >= 2.5f)
            {
                skill.Cancel();
                GainSlowdownMeter(skill.SlowdownMeterGain);
                // Do knockback burst
            }
            duration -= Time.deltaTime;
            if (duration < 0)
            {
                if(skill)
                {
                    skill.Cancel();
                    duration = 0;
                }
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            //throw new NotImplementedException();
        }
    }
}