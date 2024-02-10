using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public class Block : BaseStatus
    {

        public Block(BaseActorBattler owner) : base(owner)
        {

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
            float modifiedDamage = damage * 0.5f;
            return modifiedDamage;
        }

        public float ModifyKnockback(float knockback, Vector3 direction)
        {
            Remove();
            return knockback * 0.5f;
        }
    }
}