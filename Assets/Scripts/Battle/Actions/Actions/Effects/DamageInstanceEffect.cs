using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Utility;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class DamageInstanceEffect : IEffect
    {
        public IHitbox HitboxEffect;
        public float PostureDamage;
        public float Damage;
        public float KnockbackForce;
        public float KnockbackForceUp;
        public float CasterBuildupGain;
        public bool AutoGetHitbox;

        public void Eval(Actor.Actor actor, BaseAction action)
        {
            var gs = action as GenericSkill;
            HitboxEffect = gs.OnStartEffects.Where(item => item is IHitbox).FirstOrDefault() as IHitbox;
            var enemies = HitboxEffect.CheckEnemiesInsideHitbox(actor);
            foreach (var enemy in enemies)
                ApplyDamageEffects(actor, enemy);
        }

        private void ApplyDamageEffects(Actor.Actor casterActor, Actor.Actor targetActor)
        {
            var targetBlocking = targetActor.state.IsBlocking();
            casterActor.ActorData.ChangeBuildup(CasterBuildupGain);
            //Apply posture
            if (PostureDamage > 0)
            {
                targetActor.ApplyPosture(PostureDamage);
            }

            // Apply Damage
            var damage = Damage + casterActor.ActorData.ATK - targetActor.ActorData.DEF;
            if (damage > 0)
            {
                var hitstop = StaticHelpers.LinearMap(damage, 0.2f, 15, 0.083f, 0.420f);
                targetActor.ApplyDamage(damage);
            };

            // Apply Knockback
            if (KnockbackForce > 0)
            {
                Vector3 direction;
                direction = (targetActor.transform.position - casterActor.transform.position).normalized;
             
                targetActor.ApplyKnockback(direction, KnockbackForce);
            }

            if (KnockbackForceUp > 0)
            {
                Vector3 upDirection = Vector3.up;
                targetActor.ApplyKnockback(upDirection, KnockbackForceUp);
            }
        }
    }
}
