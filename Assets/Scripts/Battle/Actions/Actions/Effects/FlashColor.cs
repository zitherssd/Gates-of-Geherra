using System;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class FlashColor : IEffect
    {
        public float intensity;

        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.effects.FlashWhite(intensity);
        }
    }

}

