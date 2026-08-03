using System.Linq;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Manager;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    /// <summary>
    /// Smart Approach Behavior — inspired by the "Beyond the Kung-Fu Circle" pattern
    /// (Game AI Pro Ch.28, Michael Dawe) and group-combat AI from Batman Arkham /
    /// Shadow of Mordor.
    ///
    /// Core idea: each enemy holds a **stable virtual slot** on a ring around the
    /// player. The slot angle is cached per instance and only recalculated when the
    /// number of alive enemies changes, eliminating per-frame jitter.
    ///
    /// - Far from player: NavMesh to player, slight lateral nudge for spread.
    /// - Close to player: NavMesh to ring position, maintain surround stance.
    /// - At stop distance: stop and return Failure (attack behavior takes over).
    ///
    /// Few enemies → 180° frontal arc. 5+ enemies → 360° full surround.
    /// </summary>
    public class SmartApproachBehavior : BTNode
    {
        private const float StopDistance = 1.0f;
        private const float RepathInterval = 0.05f;
        private const float StuckTimeout = 1.5f;
        private const float StuckDistanceThreshold = 0.3f;
        private const float SeparationRadius = 1.2f;
        private const float SeparationStrength = 0.8f;
        private const float RingRadius = 1.2f;           // Ring hold distance from player
        private const float CloseRangeThreshold = 3f;    // Switch to ring targeting
        private const int HighDensityThreshold = 5;

        // --- Cached slot angle (stable across frames) ---
        private float cachedAngleDeg;
        private int cachedEnemyCount = -1;

        private Vector3 lastPosition;
        private float stuckTimer;
        private float repathTimer;

        /// <summary>
        /// Compute a stable slot angle for this enemy. Uses the actor's instance ID
        /// as a stable hash so the slot stays the same even if list order changes.
        /// Only recomputes when enemy count changes.
        /// </summary>
        private float GetSlotAngle(Actor actor, int enemyCount)
        {
            if (enemyCount == cachedEnemyCount)
                return cachedAngleDeg;

            cachedEnemyCount = enemyCount;

            float arcSpan = enemyCount >= HighDensityThreshold ? 360f : 180f;
            float angleStep = arcSpan / Mathf.Max(enemyCount, 1);
            float baseAngle = -arcSpan * 0.5f;

            // Use actor's persistent instance ID as stable hash
            int hash = actor.GetInstanceID();
            int slot = Mathf.Abs(hash) % Mathf.Max(enemyCount, 1);
            cachedAngleDeg = baseAngle + angleStep * slot;

            return cachedAngleDeg;
        }

        /// <summary>
        /// Compute a ring-hold position: a point at RingRadius from the player in
        /// the direction of this enemy's slot angle (relative to the player-forward).
        /// </summary>
        private Vector3 GetRingPosition(Vector3 playerPos, Vector3 toPlayer, float angleDeg)
        {
            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector3 right = Vector3.Cross(toPlayer, Vector3.up).normalized;
            Vector3 forward = toPlayer;
            Vector3 dir = (right * Mathf.Sin(angleRad) + forward * Mathf.Cos(angleRad)).normalized;
            return playerPos + dir * RingRadius;
        }

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
            Vector3 currentPos = actor.transform.position;

            // 2. Count alive enemies
            var allEnemies = BattleManager.instance.EnemyActors
                .Where(e => !e.Runtime.isDead())
                .ToList();
            int enemyCount = allEnemies.Count;

            // 3. Get stable cached slot angle
            float myAngleDeg = GetSlotAngle(actor, enemyCount);

            // 4. Stuck detection
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

            Vector3 navmeshTarget;

            // 5. Choose targeting mode based on distance
            if (distance > CloseRangeThreshold || isStuck)
            {
                // FAR / STUCK MODE: Path directly to player. No lateral nudge —
                // enemies are already naturally spread from their spawn positions.
                // Artificial nudging here fights the NavMeshAgent's pathfinding
                // and causes orbiting.
                navmeshTarget = targetPosition;

                repathTimer -= Time.deltaTime;
                if (repathTimer <= 0f)
                {
                    actor.movement.agent.SetDestination(navmeshTarget);
                    repathTimer = RepathInterval;
                }

                Vector3 navmeshDirection = actor.movement.agent.desiredVelocity.normalized;

                // Separation only — no lateral nudge
                Vector3 separation = ComputeSeparation(actor, allEnemies, currentPos);
                Vector3 finalDirection = (navmeshDirection + separation).normalized;

                return ApplyMovement(actor, isMoving, moveAction, finalDirection);
            }
            else
            {
                // CLOSE MODE: Path to ring position around the player.
                // The ring position itself creates the surround spread — no
                // additional direction nudge needed.
                Vector3 ringPos = GetRingPosition(targetPosition, toEnemy, myAngleDeg);

                repathTimer -= Time.deltaTime;
                if (repathTimer <= 0f)
                {
                    actor.movement.agent.SetDestination(ringPos);
                    repathTimer = RepathInterval;
                }

                Vector3 navmeshDirection = actor.movement.agent.desiredVelocity.normalized;

                // Separation only
                Vector3 separation = ComputeSeparation(actor, allEnemies, currentPos);
                Vector3 finalDirection = (navmeshDirection + separation).normalized;

                return ApplyMovement(actor, isMoving, moveAction, finalDirection);
            }
        }

        private Vector3 ComputeSeparation(Actor actor, System.Collections.Generic.List<Actor> allEnemies, Vector3 currentPos)
        {
            Vector3 separation = Vector3.zero;
            foreach (var ally in allEnemies)
            {
                if (ally == actor) continue;
                Vector3 toAlly = currentPos - ally.transform.position;
                float allyDist = toAlly.magnitude;
                if (allyDist < SeparationRadius && allyDist > 0.01f)
                {
                    separation += toAlly.normalized * (SeparationStrength / Mathf.Max(allyDist, 0.1f));
                }
            }
            return separation * 0.15f;
        }

        private NodeState ApplyMovement(Actor actor, bool isMoving, MoveAction moveAction, Vector3 direction)
        {
            if (isMoving)
            {
                moveAction.Direction = direction;
                return NodeState.Sucess;
            }
            else
            {
                var moveSkill = actor.Runtime.actions.OfType<MoveAction>().FirstOrDefault();
                if (moveSkill == null) return NodeState.Failure;

                moveSkill.Direction = direction;
                actor.UseAction(moveSkill);
                return NodeState.Sucess;
            }
        }
    }
}
