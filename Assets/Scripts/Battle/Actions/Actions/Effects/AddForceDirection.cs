using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class AddForceDirection : IEffect
    {
        public float Force;


        public void Eval(Actor.Actor actor, BaseAction t)
        {
            if(actor.isControllable)
            {
            Debug.Log($"Applying force in direction {t.Direction.normalized} with magnitude {Force}");
            }
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

    [Serializable]
    public class SetFriction : IEffect, IEndableEffect
    {
        public float Friction;

        public void End(Actor.Actor actor, BaseAction action)
        {
            actor.movement.SetFriction();
        }

        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.movement.SetFriction(Friction);
        }
}
[Serializable]
    public class ResestFriction : IEffect
    {
        public float Friction;

        public void Eval(Actor.Actor actor, BaseAction action)
        {
            actor.movement.SetFriction();
        }
}

}

