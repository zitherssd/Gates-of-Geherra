using System;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Status;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Effects
{
    /// <summary>
    /// Applies a status to each enemy actually hit by an attack. Add to a
    /// DamageEffect's OnHitEffects list. Unlike AddStatusEffect (which targets the
    /// caster or the closest enemy), this runs per hit target in a 1vN arena.
    /// </summary>
    [Serializable]
    public class AddStatusOnHitEffect : IOnHitEffect
    {
        public BaseStatus StatusToApply;

        public void Eval(Actor.Actor target, Actor.Actor caster, BaseAction action)
        {
            if (StatusToApply == null || target == null) return;

            var statusInstance = UnityEngine.Object.Instantiate(StatusToApply);
            target.statusManager.Add(statusInstance);
        }
    }
}
