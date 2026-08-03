﻿﻿﻿using System.Linq;
using Assets.Scripts.Battle.Actions.Actions;
using Assets.Scripts.Battle.Manager;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class CircleApproachBehavior : BTNode
    {
        private const float StopDistance = 1f;
        private const float RepathInterval = 0.2f;
        private const float StuckTimeout = 1.5f;
        private const float StuckDistanceThreshold = 0.3f;

        private Vector3 lastPosition;
        private float stuckTimer;
        private float repathTimer;

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            MoveAction moveAction;
            bool isMoving = actor.state.IsMoving(out moveAction);

            float distance = actor.target.DistanceToClosestEnemy;

            // 1. Arrive: stop if close enough to attack
            if (distance < StopDistance)
            {
                if (isMoving)
                {
                    var currentAction = actor.GetCurrentAction();
                    if (currentAction != null && currentAction == moveAction)
                        currentAction.EndAction(Actions.ActionEndReason.Completed);
                }
                return NodeState.Failure;
            }

            Vector3 toEnemy = actor.target.DirectionToClosestEnemy.normalized;
            Vector3 targetPosition = actor.target.TargetPosition;
            var allies = BattleManager.instance.EnemyActors
                .Where(a => a != actor && !a.Runtime.isDead())
                .ToList();

            // 2. Detect stuck agent — if we haven't moved enough, try a sharper flank
            Vector3 currentPos = actor.transform.position;
            float movedThisFrame = (currentPos - lastPosition).magnitude;
            lastPosition = currentPos;

            bool isStuck = false;
            if (movedThisFrame < StuckDistanceThreshold * Time.deltaTime)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer >= StuckTimeout)
                    isStuck = true;
            }
            else
            {
                stuckTimer = 0f;
            }

            // 3. Check if path to player is blocked by an ally
            Actor blockingAlly = null;
            float closestBlockedDist = float.MaxValue;
            foreach (var ally in allies)
            {
                Vector3 toAlly = ally.transform.position - currentPos;
                float dist = toAlly.magnitude;
                if (dist < 3.5f && dist > 0.1f)
                {
                    Vector3 toAllyNorm = toAlly / dist;
                    if (Vector3.Dot(toEnemy, toAllyNorm) > 0.8f && dist < closestBlockedDist)
                    {
                        blockingAlly = ally;
                        closestBlockedDist = dist;
                    }
                }
            }

            Vector3 approachDirection;

            if (blockingAlly != null)
            {
                // 4. Go around the blocking ally, then resume approach
                Vector3 toAlly = (blockingAlly.transform.position - currentPos).normalized;
                float crossY = Vector3.Cross(toEnemy, toAlly).y;
                float dodgeSign = crossY > 0 ? -1f : 1f;

                Vector3 tangent = Vector3.Cross(toEnemy, Vector3.up).normalized * dodgeSign;
                float forwardWeight = Mathf.Clamp01((closestBlockedDist - 1.0f) / 2.5f);

                approachDirection = (toEnemy * forwardWeight + tangent * (1f - forwardWeight));
            }
            else
            {
                // 5. Path is free — advance toward the player with a slight flank offset
                // so enemies spread out around the player instead of stacking
                Vector3 rightTangent = Vector3.Cross(Vector3.up, toEnemy).normalized;

                float rightCount = 0f;
                float leftCount = 0f;
                foreach (var ally in allies)
                {
                    Vector3 allyToTarget = (ally.transform.position - targetPosition).normalized;
                    float sideDot = Vector3.Dot(allyToTarget, rightTangent);
                    if (sideDot > 0.3f)
                        rightCount += 1f;
                    else if (sideDot < -0.3f)
                        leftCount += 1f;
                }

                // If stuck, bias harder in one direction to break out
                float stuckBias = isStuck ? 0.6f : 0f;

                // Pick the less crowded side, but keep it subtle — mostly move forward
                float flankSign = rightCount > leftCount ? -1f : 1f;
                float flankBias = Mathf.Clamp01(Mathf.Abs(rightCount - leftCount) * 0.15f) + stuckBias;

                approachDirection = (toEnemy * (1f - flankBias) + rightTangent * flankBias * flankSign);
            }

            approachDirection.y = 0f;
            approachDirection.Normalize();

            // 6. Set destination ahead along the approach direction — far enough for the
            // NavMeshAgent to produce meaningful desiredVelocity.
            float destinationDistance = Mathf.Max(distance * 0.8f, 2.0f);
            Vector3 moveTarget = currentPos + approachDirection * destinationDistance;

            // 7. Re-path sparingly — not every frame
            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                var agent = actor.movement.agent;
                agent.SetDestination(moveTarget);
                repathTimer = RepathInterval;
            }

            var navmeshDirection = actor.movement.agent.desiredVelocity.normalized;

            // 8. Apply movement at full speed — no arrive slowdown
            if (isMoving)
            {
                moveAction.Direction = navmeshDirection;
                return NodeState.Sucess;
            }
            else
            {
                var moveSkill = actor.Runtime.actions.OfType<MoveAction>().FirstOrDefault();
                if (moveSkill == null) return NodeState.Failure;

                moveSkill.Direction = navmeshDirection;
                actor.UseAction(moveSkill);
                return NodeState.Sucess;
            }
        }
    }
}
