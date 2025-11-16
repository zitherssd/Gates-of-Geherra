using System;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class ClearHitbox : IEffect
    {
        public void Eval(Actor.Actor actor, BaseAction action)
        {
            actor.effects.ClearHitbox();
        }
    }

}
