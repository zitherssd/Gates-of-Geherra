 using Assets.Scripts.Battle.Actions;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.AI.Behaviors
{
    public class FlankApproachBehavior : IAIBehavior
    {
        public bool Execute(AISystem ai, Actor actor)
        {
            // If already moving or close to the enemy, don't execute
            if (actor.Rb.velocity.magnitude > 0.5f || actor.target.DistanceToClosestEnemy < 1.4f)
                return false;

            var moveSkill = actor.ActorData.actions.OfType<MoveAction>().FirstOrDefault();
            if (moveSkill == null) return false;

            // Get the direction to the closest enemy
            Vector3 toEnemy = actor.target.DirectionToClosestEnemy.normalized;

            // Compute a perpendicular direction (flanking direction)
            Vector3 flankDirection = Vector3.Cross(toEnemy, Vector3.up).normalized;

            // Randomly choose left (-flankDirection) or right (+flankDirection)
            if (UnityEngine.Random.value > 0.5f) flankDirection = -flankDirection;

            // Combine forward movement and flanking movement
            Vector3 approachDirection = (toEnemy * 0.5f + flankDirection * 0.5f).normalized;

            moveSkill.Direction = approachDirection;
            actor.UseAction(moveSkill, actor.state.TransitionToIdle);
            return true;
        }
    }

}
