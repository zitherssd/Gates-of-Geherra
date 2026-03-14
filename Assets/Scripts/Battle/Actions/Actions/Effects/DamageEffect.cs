using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Utility;
using System;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.Battle.Actions.BaseAction;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class DamageEffect : IEffect, IItemEffect
    {
        public bool UseHitbox = true;
        public IHitbox HitboxEffect; //How to get hitbox?
        public DamageInstance DamageData;


        public virtual void Eval(Actor.Actor actor, BaseAction action)
        {
            var gs = action as GenericSkill;
            HitboxEffect = gs.OnStartEffects.Where(item => item is IHitbox).FirstOrDefault() as IHitbox;
            var enemies = HitboxEffect.CheckEnemiesInsideHitbox(actor);
            foreach (var enemy in enemies)
                ApplyDamageEffects(actor, enemy, action);
        }

        public void Eval(Actor.Actor actor)
        {
            ApplyDamageEffects(actor, actor, null);
        }

        public void ApplyDamageEffects(Actor.Actor casterActor, Actor.Actor targetActor, BaseAction action)
        {
            // Need ro revise
            var targetBlocking = targetActor.state.IsBlocking();
            if (targetBlocking)
                casterActor.ActorData.ChangeBuildup(DamageData.BuildupGainOnHit / 2);
            else
                casterActor.ActorData.ChangeBuildup(DamageData.BuildupGainOnHit);

            targetActor.ApplyDamageInstance(DamageData, casterActor, action);
        }
    }
}
