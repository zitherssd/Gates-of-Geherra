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
            if(StatusToApply != null && finalTarget != null)
            {
                //var statusInstance = Instantiate(StatusToApply);
                //finalTarget.statusManager.Add(statusInstance);
            }
        }
    }
}