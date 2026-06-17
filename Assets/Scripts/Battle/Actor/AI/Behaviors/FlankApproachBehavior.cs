﻿using System.Linq;
using Assets.Scripts.Battle.Actions.Actions;
using Assets.Scripts.Battle.Manager;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class CircleApproachBehavior : BTNode
    {
        private float dodgeDirectionSign = 1f;
        private float directionChangeTimer = 0f;

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            MoveAction moveAction;
            bool isMoving = actor.state.IsMoving(out moveAction);

            // 1. Stop moving if we've reached the target distance
            if (actor.target.DistanceToClosestEnemy < 1.4f)
            {
                if (isMoving)
                {
                    var currentAction = actor.GetCurrentAction();
                    if (currentAction != null && currentAction == moveAction)
                    {
                        currentAction.EndAction(Actions.ActionEndReason.Completed);
                    }
                }
                return NodeState.Failure;
            }

            Vector3 toEnemy = actor.target.DirectionToClosestEnemy.normalized;
            var allies = BattleManager.instance.EnemyActors.Where(a => a != actor && !a.Runtime.isDead()).ToList();

            // 2. Check if path towards player is free
            Actor blockingAlly = null;
            float closestBlockedDist = float.MaxValue;

            foreach (var ally in allies)
            {
                Vector3 toAlly = ally.transform.position - actor.transform.position;
                float dist = toAlly.magnitude;

                // If an ally is within 3.5 meters
                if (dist < 3.5f && dist > 0.1f)
                {
                    Vector3 toAllyNorm = toAlly / dist;
                    // And they are roughly in front of us (Dot > 0.8 is roughly within a 36-degree cone)
                    if (Vector3.Dot(toEnemy, toAllyNorm) > 0.8f && dist < closestBlockedDist)
                    {
                        blockingAlly = ally;
                        closestBlockedDist = dist;
                    }
                }
            }

            Vector3 approachDirection;

            if (blockingAlly == null)
            {
                // 3. Path is free, beeline for the player
                approachDirection = toEnemy;
                directionChangeTimer = 0f; // Reset timer so we make a fresh choice next time we're blocked
            }
            else
            {
                // 4. Ally in the way, go around them
                directionChangeTimer -= Time.deltaTime;
                if (directionChangeTimer <= 0f)
                {
                    Vector3 toAlly = (blockingAlly.transform.position - actor.transform.position).normalized;
                    // Cross product tells us if they are slightly to our left or right
                    float crossY = Vector3.Cross(toEnemy, toAlly).y;
                    
                    // Dodge in the opposite direction
                    dodgeDirectionSign = crossY > 0 ? -1f : 1f;
                    directionChangeTimer = UnityEngine.Random.Range(1.0f, 2.0f);
                }

                // A vector pointing sideways from our target
                Vector3 tangent = Vector3.Cross(toEnemy, Vector3.up).normalized * dodgeDirectionSign;

                // The closer they are, the more we steer sideways instead of forward
                float forwardWeight = Mathf.Clamp01((closestBlockedDist - 1.0f) / 2.5f); 
                float sideWeight = 1.0f - forwardWeight;

                approachDirection = (toEnemy * forwardWeight + tangent * sideWeight);
            }

            approachDirection.y = 0f; // Keep on flat plane
            approachDirection.Normalize();

            // 5. Apply movement
            if (isMoving)
            {
                // Update the direction smoothly while continuing to walk
                moveAction.Direction = approachDirection;
                return NodeState.Sucess;
            }
            else
            {
                // Start a new MoveAction
                var moveSkill = actor.Runtime.actions.OfType<MoveAction>().FirstOrDefault();
                if (moveSkill == null) return NodeState.Failure;

                moveSkill.Direction = approachDirection;
                actor.UseAction(moveSkill);
                return NodeState.Sucess;
            }
        }
    }
}
