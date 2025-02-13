using UnityEditor;
using UnityEngine;
using Assets.Scripts.Pattern;
using System;
using Assets.Scripts.Battle.Actions.Reactions;

namespace Assets.Scripts.Battle.Components.State.States
{
    public class BlockState : IState
    {
        private readonly Actor owner;

        [Range(0, 2)] public float DamageModifier = 1f;
        [Range(0, 2)] public float PostureModifier = 1f;
        [Range(0, 2)] public float KnockbackModifier = 1f;
        public Action onAnimationEnd;
        private float duration;
        private Block skill;

        public BlockState(Actor owner)
        {
            this.owner = owner;
        }

        public BlockState Set(Block block)
        {
            this.DamageModifier = block.DamageModifier;
            this.PostureModifier = block.PostureModifier;
            this.KnockbackModifier = block.KnockbackModifier;
            this.duration = block.duration;
            this.skill = block;
            return this;
        }
        public BlockState Set(float DamageModifier, float PostureModifier, float KnockbackModifier, float duration)
        {
            this.DamageModifier = DamageModifier;
            this.PostureModifier = PostureModifier;
            this.KnockbackModifier = KnockbackModifier;
            this.duration = duration;
            return this;
        }

        public void Enter()
        {
            owner.PlayAnimation("Block");
            owner.DamageRecieved += ModifyDamage;
            owner.KnockbackRecieved += ModifyKnockback;
            owner.PostureRecieved += ModifyPosture;
            owner.OnDamageApplied += GainSlowdownMeter;
            owner.staminaRegenRateModifier = 0.5f;
        }

        public void Exit()
        {
            owner.DamageRecieved -= ModifyDamage;
            owner.KnockbackRecieved -= ModifyKnockback;
            owner.PostureRecieved -= ModifyPosture;

            owner.PlayAnimation("Idle");
            owner.OnDamageApplied -= GainSlowdownMeter;
            owner.staminaRegenRateModifier = 1f;
        }

        private void GainSlowdownMeter(float damage)
        {
            if(owner.isControllable())
            UIManager.instance.GainMeter(skill.SlowdownMeterGain);
        }

        public float ModifyDamage(float damage)
        {
            float modifiedDamage = damage * DamageModifier;
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
            duration -= Time.deltaTime;
            if (duration < 0)
            {
                if(skill)
                {
                    skill.cancel?.Invoke();
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