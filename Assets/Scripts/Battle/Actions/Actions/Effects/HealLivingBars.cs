using Assets.Scripts.Battle.Actor;
using System;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class HealLivingBars : IEffect, IItemEffect
    {
        public void Eval(Actor.Actor actor, BaseAction action)
        {
            actor.Runtime.HealAllAliveBars();
        }

        public void Eval(Actor.Actor owner)
        {
            owner.Runtime.HealAllAliveBars();
        }
    }
    [Serializable]
    public class GainBuildup : IItemEffect
    {
        public float BuildupGain;

        public void Eval(Actor.Actor owner)
        {
            owner.Runtime.ChangeBuildup(BuildupGain);
        }
    }

}

