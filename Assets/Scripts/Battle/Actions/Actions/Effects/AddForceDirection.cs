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

}

