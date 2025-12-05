using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets.Scripts.Battle.Actions.BaseAction;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using Assets.Scripts.Battle.Actions.Skills;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class DamageTickEffect : DamageEffect, IEffect, IUpdateableEffect
    {
        float timer;
        float tickRate;

        public override void Eval(Actor.Actor actor, BaseAction action)
        {
            var gs = action as GenericSkill;
            HitboxEffect = gs.OnStartEffects.Where(item => item is IHitbox).FirstOrDefault() as IHitbox;
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
                    ApplyDamageEffects(actor, enemy, action);
            }

        }
    }
}
