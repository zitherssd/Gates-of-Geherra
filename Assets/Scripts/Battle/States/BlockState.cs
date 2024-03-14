using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public class BlockState : BaseStatus
    {
        private float damageModifier;
        private float postureModifier;
        private float knockbackModifier;

        public BlockState(BaseActorBattler owner, float damageModifier, float postureModifier, float knockbackModifier) : base(owner)
        {
            this.damageModifier = damageModifier;
            this.postureModifier = postureModifier;
            this.knockbackModifier = knockbackModifier;
        }

        public override void Apply()
        {
            owner.ApplyDamageModifiers += ModifyDamage;
            owner.ApplyKnockbackModifiers += ModifyKnockback;
        }

        public override void Remove()
        {
            base.Remove();
            owner.ApplyDamageModifiers -= ModifyDamage;
            owner.ApplyKnockbackModifiers -= ModifyKnockback;
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