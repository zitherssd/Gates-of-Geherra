using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets.Scripts.Battle.Actions.BaseAction;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class DamageTickEffect : IEffect, IUpdateableEffect
    {
        public ShowHitboxOnPlayer HitboxEffect;
        public float tickRate = 0.1f;
        private float timer;
        public float PostureDamage;
        public float Damage;
        public float KnockbackForce;
        public float CasterBuildupGain;

        public void Eval(Actor.Actor actor, BaseAction action)
        {
            timer = 0f;
        }

        public void Update(Actor.Actor actor, BaseAction action, float dt)
        {
            timer += dt;
            if (timer >= tickRate)
            {
                timer -= tickRate;

                var enemies = HitboxEffect.CheckEnemiesInsideHitbox(actor);
                foreach (var enemy in enemies)
                    ApplyDamageEffects(actor, enemy);
            }

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
        }
    }
}
