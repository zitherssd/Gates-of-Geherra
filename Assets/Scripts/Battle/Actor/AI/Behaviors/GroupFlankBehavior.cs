using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Manager;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class GroupFlankBehavior : BTNode
    {
        private const float FlankRadius = 3f;  // Desired flanking radius
        private const float MaxFlankDistance = 5f; // Distance where full flanking occurs
        private const float SeparationRadius = 1f; // Minimum distance between allies
        private const float SeparationStrength = 1.5f; // How strongly to push away from allies

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            if (actor.Rb.velocity.magnitude > 0.5f || actor.target.DistanceToClosestEnemy < 1.4f)
                return NodeState.Failure;

            // Get all allies targeting the same enemy
            List<Actor> allies = BattleManager.instance.EnemyActors
                .Where(a => a.target.ClosestEnemy == actor.target.ClosestEnemy && a != actor)
                .ToList();

            // Compute movement directions
            Vector3 toEnemy = actor.target.DirectionToClosestEnemy.normalized;
            Vector3 flankDirection = DetermineFlankDirection(actor, allies, toEnemy);
            Vector3 separationForce = ComputeSeparation(actor, allies);
            Vector3 approachDirection = (toEnemy * 0.3f + flankDirection * 0.5f + separationForce * 0.2f).normalized;

            var agent = actor.movement.agent;
            agent.SetDestination(actor.transform.position + approachDirection);
            var navmeshDirection = agent.desiredVelocity.normalized;

            // If already moving, update direction
            MoveAction moveAction;
            if (actor.state.IsMoving(out moveAction))
            {
                moveAction.Direction = navmeshDirection;
                return NodeState.Sucess;
            }

            // Start moving if not already moving
            moveAction = actor.Runtime.actions.OfType<MoveAction>().FirstOrDefault();
            if (moveAction == null) return NodeState.Failure;
            moveAction.Direction = navmeshDirection;
            actor.UseAction(moveAction);
            return NodeState.Sucess;
        }

        private Vector3 DetermineFlankDirection(Actor actor, List<Actor> allies, Vector3 toEnemy)
        {
            float distance = actor.target.DistanceToClosestEnemy;

            // Sigmoid-like function for gradual flanking increase
            float flankWeight = 1f - Mathf.Exp(-distance / MaxFlankDistance);

            // Choose clockwise or counterclockwise flanking
            bool clockwise = ShouldMoveClockwise(actor, allies);
            Vector3 tangentDirection = Vector3.Cross(toEnemy, Vector3.up).normalized * (clockwise ? 1 : -1);

            return (toEnemy * (1 - flankWeight) + tangentDirection * flankWeight).normalized;
        }

        private Vector3 ComputeSeparation(Actor actor, List<Actor> allies)
        {
            Vector3 separation = Vector3.zero;
            foreach (var ally in allies)
            {
                float distance = Vector3.Distance(actor.transform.position, ally.transform.position);
                if (distance < SeparationRadius && distance > 0)
                {
                    Vector3 pushAway = (actor.transform.position - ally.transform.position).normalized / distance;
                    separation += pushAway * SeparationStrength;
                }
            }
            return separation.normalized;
        }

        private bool ShouldMoveClockwise(Actor actor, List<Actor> allies)
        {
            int index = allies.IndexOf(actor);
            return index % 2 == 0;
        }
    }
}
