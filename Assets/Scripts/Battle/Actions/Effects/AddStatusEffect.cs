using System;
using Assets.Scripts.Battle.Actions.Effects;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Status;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Effects
{
    public enum StatusTarget
    {
        Caster,
        Target
    }

    [Serializable]
    public class AddStatusEffect : IEffect
    {
        public BaseStatus StatusToApply;
        public StatusTarget ApplyTo = StatusTarget.Caster;

        public void Eval(Actor.Actor actor, BaseAction action)
        {
            var target = actor.target.ClosestEnemy;
            var finalTarget = (ApplyTo == StatusTarget.Caster) ? actor : target;
            if (StatusToApply != null && finalTarget != null)
            {
                var statusInstance = UnityEngine.Object.Instantiate(StatusToApply);
                finalTarget.statusManager.Add(statusInstance);
            }
        }
    }

    /// <summary>
    /// Grants invincibility (InvincibilityStatus) to the caster for the action's lifetime.
    /// Implements IEndableEffect so the status is auto-removed on action cleanup —
    /// a safety net if the action is interrupted mid-window. Pair with RemoveStatusEffect
    /// in a later hit window for a precise frame window (e.g. Weave frames 2–8).
    /// </summary>
    [Serializable]
    public class AddInvincible : IEffect, IEndableEffect
    {
        public void Eval(Actor.Actor actor, BaseAction action)
        {
            if (actor.statusManager.HasStatus<InvincibilityStatus>()) return;
            var status = ScriptableObject.CreateInstance<InvincibilityStatus>();
            actor.statusManager.Add(status);
        }

        public void End(Actor.Actor actor, BaseAction action)
        {
            var active = actor.statusManager.GetStatus<InvincibilityStatus>();
            if (active != null)
                actor.statusManager.Remove(active);
        }
    }
}