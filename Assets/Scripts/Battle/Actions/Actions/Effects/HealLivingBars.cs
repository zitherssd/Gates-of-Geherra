using System;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class HealLivingBars : IEffect, IItemEffect
    {
        public void Eval(Actor.Actor actor, BaseAction action)
        {
            actor.ActorData.HealAllAliveBars();
        }

        public void Eval(Actor.Actor owner)
        {
            owner.ActorData.HealAllAliveBars();
        }
    }

}

