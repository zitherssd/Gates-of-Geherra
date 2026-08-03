using System;

namespace Assets.Scripts.Battle.Actions.Effects
{
    [Serializable]
    public class HealEffect : IItemEffect
    {
        public void Eval(Actor.Actor owner)
        {
            owner.Runtime.HealAllBars();
        }
    }
}
