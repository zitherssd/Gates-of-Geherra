using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.Status

{
    public class BlockStatus : BaseStatus
    {
        private float damageModifier;
        private float postureModifier;
        private float knockbackModifier;

        public BlockStatus(float damageModifier, float postureModifier, float knockbackModifier)
        {
            this.damageModifier = damageModifier;
            this.postureModifier = postureModifier;
            this.knockbackModifier = knockbackModifier;
        }

        public override void Apply()
        {
            owner.DamageRecieved += ModifyDamage;
            owner.KnockbackRecieved += ModifyKnockback;
        }

        public override void Remove()
        {
            base.Remove();
            owner.DamageRecieved -= ModifyDamage;
            owner.KnockbackRecieved -= ModifyKnockback;
        }

        public float ModifyDamage(float damage)
        {
            float modifiedDamage = damage * damageModifier;
            return modifiedDamage;
        }

        public float ModifyKnockback(float knockback, Vector3 direction)
        {
            Remove();
            return knockback * knockbackModifier;
        }
    }
}