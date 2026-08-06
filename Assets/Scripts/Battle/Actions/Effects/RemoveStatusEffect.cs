using System;
using System.Linq;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Status;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Effects
{
    /// <summary>
    /// Removes an active status (by type) from the caster or the closest enemy.
    /// Needed to turn a status OFF — e.g. invincibility granted in hit window 1
    /// and revoked in hit window 2. Statuses are Instantiate'd clones at runtime,
    /// so we match by type rather than by reference.
    /// </summary>
    [Serializable]
    public class RemoveStatusEffect : IEffect
    {
        public BaseStatus StatusToRemove;
        public StatusTarget ApplyTo = StatusTarget.Caster;

        public void Eval(Actor.Actor actor, BaseAction action)
        {
            var target = actor.target.ClosestEnemy;
            var finalTarget = (ApplyTo == StatusTarget.Caster) ? actor : target;
            if (StatusToRemove == null || finalTarget == null) return;

            var active = finalTarget.statusManager.activeStatuses
                .FirstOrDefault(s => s != null && s.GetType() == StatusToRemove.GetType());
            if (active != null)
            {
                finalTarget.statusManager.Remove(active);
            }
        }
    }
}
