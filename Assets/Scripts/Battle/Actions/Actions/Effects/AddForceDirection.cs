using System;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class AddForceDirection : IEffect
    {
        public float Force;


        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.movement.AddForce(t.Direction.normalized * Force);
        }

        enum TARGET { }
    }

    [Serializable]
    public class AddForceTarget : IEffect
    {
        public float Force;

        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.movement.AddForce(actor.target.DirectionToClosestEnemy.normalized * Force);
        }

        enum TARGET { }
    }

}

