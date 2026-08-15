using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Effects
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

    /// <summary>
    /// Applies a small force in a random horizontal direction.
    /// Used for INSTANT actions (no directional input), e.g. the Weave dodge.
    /// </summary>
    [Serializable]
    public class AddRandomForce : IEffect
    {
        public float Force;

        public void Eval(Actor.Actor actor, BaseAction action)
        {
            var randomDir = UnityEngine.Random.insideUnitCircle; // (x, y) in unit circle
            var dir = new Vector3(randomDir.x, 0f, randomDir.y).normalized;
            actor.movement.AddForce(dir * Force);
        }
    }

    /// <summary>
    /// Applies a force in the direction the camera is looking, projected onto the
    /// horizontal plane — i.e. away from the camera. Used for INSTANT actions
    /// (no directional input), e.g. the Weave dodge hop.
    /// </summary>
    [Serializable]
    public class AddForceAwayCamera : IEffect
    {
        public float Force;

        public void Eval(Actor.Actor actor, BaseAction action)
        {
            var camera = Camera.main;
            if (camera == null) return;

            var dir = camera.transform.forward;
            dir.y = 0f;
            dir.Normalize();

            if (actor.isControllable)
            {
                Debug.Log($"Applying force away from camera {dir} with magnitude {Force}");
            }

            actor.movement.AddForce(dir * Force);
        }
    }

}

