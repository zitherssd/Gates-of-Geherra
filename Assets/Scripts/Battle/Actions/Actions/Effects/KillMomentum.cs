using System;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class KillMomentum : IEffect
    {
        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.movement.ResetMomentum();
        }
    }

}

