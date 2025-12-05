using System;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class HealEffect : IItemEffect
    {
        public void Eval(Actor.Actor owner)
        {
            owner.ActorData.HealAllBars();
        }
    }
}
