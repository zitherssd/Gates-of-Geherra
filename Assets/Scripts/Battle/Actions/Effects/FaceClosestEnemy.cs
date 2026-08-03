using System;

namespace Assets.Scripts.Battle.Actions.Effects
{
    [Serializable]
    public class FaceClosestEnemy : IEffect
    {
        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.movement.FaceTarget(actor.target.ClosestEnemy);
        }
    }

}

