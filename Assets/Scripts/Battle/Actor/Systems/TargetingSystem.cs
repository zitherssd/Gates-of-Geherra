using System.Linq;
using Assets.Scripts.Utility;
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
                    Debug.LogWarning("Target is null! Returning actor's position instead.");
                    return actor.transform.position; // Fallback to the actor's position or a default value
                }
            }
        }


        public Actor ClosestEnemy
        {
            get
            {
                if (actor.isControllable)
                    return BattleManager.instance.EnemyActors.OrderBy(enemyActor => (enemyActor.transform.position - actor.transform.position).magnitude).Where(actor => actor.state.IsAlive()).First();
                else
                    return BattleManager.instance.PlayerActors.OrderBy(playerActor => (playerActor.transform.position - actor.transform.position).magnitude).Where(actor => actor.state.IsAlive()).First();
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
            if (!actor.isControllable) target = ClosestEnemy;
            //if it has no target
            if (target == null)
            {
                //get the closest enemy 
                var possibleTargets = BattleManager.instance.EnemyActors.Where(actor => actor.state.IsAlive()).ToList();
                var closestTarget = possibleTargets.OrderBy(actor => actor.target.DistanceToClosestEnemy).FirstOrDefault();
                if (closestTarget != null) target = closestTarget;
                if (actor.isControllable) selectionCircle.target = actor.transform;
                else
                    target = BattleManager.instance.PlayerActors[0];
            }
            else
            {
                var possibleTargets = BattleManager.instance.EnemyActors
                    .Where(actor => actor.state.CurrentState != actor.state.deathState)
                    .ToList();

                var closestTarget = possibleTargets
                    .OrderBy(actor => actor.target.DistanceToClosestEnemy)
                    .FirstOrDefault();

                target = closestTarget;
            }
            if (actor.isControllable && target) selectionCircle.target = target.transform;

        }
        //if it's has a target but another enemy is even closer
    }
}
