using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public class BlockStatus : BaseStatus
    {
        private float damageModifier;
        private float postureModifier;
        private float knockbackModifier;

        public BlockStatus(Actor owner, float damageModifier, float postureModifier, float knockbackModifier) : base(owner)
        {
            this.damageModifier = damageModifier;
            this.postureModifier = postureModifier;
            this.knockbackModifier = knockbackModifier;
        }

        public override void Apply()
        {
            owner.DamageDealt += ModifyDamage;
            owner.KnockbackDealt += ModifyKnockback;
        }

        public override void Remove()
        {
            base.Remove();
            owner.DamageDealt -= ModifyDamage;
            owner.KnockbackDealt -= ModifyKnockback;
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