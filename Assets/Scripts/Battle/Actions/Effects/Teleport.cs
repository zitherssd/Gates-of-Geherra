using System;

namespace Assets.Scripts.Battle.Actions.Effects
{
    [Serializable]
    public class Teleport : IEffect
    {
        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.transform.position += t.Direction * t.StickMult;

        }
    }

}

