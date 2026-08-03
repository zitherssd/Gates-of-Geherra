using System;

namespace Assets.Scripts.Battle.Actions.Effects
{
    [Serializable]
    public class StaminaHealEffect : IEffect
    {
        public float HealAmount;

        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.Runtime.DealStaminaDamage(-HealAmount);
        }
    }

}

