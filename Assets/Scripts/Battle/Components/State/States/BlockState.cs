using UnityEditor;
using UnityEngine;
using Assets.Scripts.Pattern;
using System;

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

        public BlockState(Actor owner)
        {
            this.owner = owner;
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
            Time.timeScale = 1f;
        }

        public void Exit()
        {
            owner.DamageRecieved -= ModifyDamage;
            owner.KnockbackRecieved -= ModifyKnockback;
        }

        public float ModifyDamage(float damage)
        {
            float modifiedDamage = damage * DamageModifier;
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
                owner.state.TransitionTo(owner.state.idleState);
                duration = 0;
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            throw new NotImplementedException();
        }
    }
}