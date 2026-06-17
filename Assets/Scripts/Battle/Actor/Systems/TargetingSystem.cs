using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Utility;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.Systems
{
    public class TargetingSystem
    {
        private readonly Actor actor;
        public Actor target;
        private SelectionCircle selectionCircle;

        public TargetingSystem(Actor actor)
        {
            this.actor = actor;
            selectionCircle = actor.GetComponentInChildren<SelectionCircle>();
        }

        public Vector3 TargetPosition
        {
            get
            {
                if (target != null)
                {
                    return target.transform.position;
                }
                else
                {
                    return actor.transform.position + actor.transform.forward * 2; // Fallback to the actor's position or a default value
                }
            }
        }

        public Vector3 LargestDirectionFromEnemies()
        {
             if (actor.isControllable)
            {
                var allAlivEnemies = BattleManager.instance.EnemyActors.Where(actor => !actor.Runtime.isDead()).ToList();
                if(allAlivEnemies.Count > 0)
                {
                var furthestenemiy = allAlivEnemies.OrderBy(enemy => (enemy.transform.position - actor.transform.position).magnitude);
                return furthestenemiy.LastOrDefault().transform.position - actor.transform.position;
                }
                else
                return actor.transform.position + actor.transform.forward * 1;
            }
            return Vector2.zero;
        }

        public Vector3 GetWeightedAverageEnemyPosition()
        {
            var allAliveEnemies = BattleManager.instance.EnemyActors.Where(enemy => !enemy.Runtime.isDead()).ToList();

            if (allAliveEnemies.Count == 0)
            {
                // No enemies, return a position in front of the actor
                return actor.transform.position + actor.transform.forward * 2f;
            }

            Vector3 weightedPositionSum = Vector3.zero;
            float totalWeight = 0f;

            foreach (var enemy in allAliveEnemies)
            {
                float distance = Vector3.Distance(actor.transform.position, enemy.transform.position);

                // Using inverse distance for weight. Closer enemies get higher weight.
                // Add a small epsilon to avoid division by zero.
                float weight = 1.0f / (distance + 1e-6f);

                weightedPositionSum += enemy.transform.position * weight;
                totalWeight += weight;
            }

            return totalWeight > 0f ? weightedPositionSum / totalWeight : allAliveEnemies[0].transform.position;
        }

        public Actor ClosestEnemy
        {
            get
            {
                if (actor.isControllable)


                    return BattleManager.instance.EnemyActors.OrderBy(enemyActor => (enemyActor.transform.position - actor.transform.position).magnitude).Where(actor => !actor.Runtime.isDead()).First();
                else
                    return BattleManager.instance.PlayerActors[0];
            }
        }

        public Vector3 DirectionToClosestEnemy
        {
            get
            {
                return (ClosestEnemy.transform.position - actor.transform.position).normalized;
            }
        }
        public float DistanceToClosestEnemy
        {
            get
            {
                return (ClosestEnemy.transform.position - actor.transform.position).magnitude;
            }
        }

        internal void Update()
        {
            var previousTarget = target;

            if (!actor.isControllable)
            {
                target = ClosestEnemy;
                return;
            }
            if (target == null)
            {
                //get the closest enemy 
                var possibleTargets = BattleManager.instance.EnemyActors.Where(actor => !actor.Runtime.isDead()).ToList();
                var closestTarget = possibleTargets.OrderBy(actor => actor.target.DistanceToClosestEnemy).FirstOrDefault();
                if (closestTarget != null)
                {
                    target = closestTarget;
                }
                if (actor.isControllable) selectionCircle.target = actor.transform;
                else
                    target = BattleManager.instance.PlayerActors[0];
            }
            else
            {
                var possibleTargets = BattleManager.instance.EnemyActors
                    .Where(actor => !actor.Runtime.isDead())
                    .ToList();


                var closestTarget = possibleTargets
                    .OrderBy(actor => actor.target.DistanceToClosestEnemy)
                    .FirstOrDefault();
                if (target != closestTarget) CameraManager.instance.SlowTrack = true;
                target = closestTarget;
            }
            if (actor.isControllable && target) selectionCircle.target = target.transform;

        }
        //if it's has a target but another enemy is even closer
    }
}
