using System;

namespace Assets.Scripts.Battle.Actions.Effects
{
    [Serializable]
    public class FaceDirection : IEffect
    {
        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.movement.FaceDirection(t.Direction.normalized);
        }
    }

}

