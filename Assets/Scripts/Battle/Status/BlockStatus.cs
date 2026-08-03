using Assets.Scripts.Battle.Manager;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.Status

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
            //owner.DamageRecieved += ModifyDamage;
            //owner.KnockbackRecieved += ModifyKnockback;
            BattleManager.instance.OnNewTurn += Remove;
        }

        public void Remove(uint turncount)
        {
            base.Remove();
            //owner.DamageRecieved -= ModifyDamage;
            //owner.KnockbackRecieved -= ModifyKnockback;
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