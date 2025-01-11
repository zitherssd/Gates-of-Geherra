using Assets.Scripts.Utility;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.Target
{
    public class TargetingManager
    {
        private readonly Actor actor;
        public Actor target;
        private SelectionCircle selectionCircle;

        public TargetingManager(Actor actor)
        {
            this.actor = actor;
            selectionCircle = actor.GetComponentInChildren<SelectionCircle>();
        }

        public Vector3 TargetPosition { get {
                return target.transform.position;
            } }


    public Actor ClosestEnemy
        {
            get
            {
                if (actor.isControllable())
                    return BattleManager.instance.EnemyActors.OrderBy(enemyActor => (enemyActor.transform.position - actor.transform.position).magnitude).First();
                else
                    return BattleManager.instance.PlayerActors.OrderBy(playerActor => (playerActor.transform.position - actor.transform.position).magnitude).First();
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
            //if it has no target
            if (target == null)
            {
                //get the closest enemy 
                var possibleTargets = BattleManager.instance.EnemyActors.Where(actor => actor.state.CurrentState != actor.state.deathState).ToList();
                var closestTarget = possibleTargets.OrderBy(actor => actor.target.DistanceToClosestEnemy).FirstOrDefault();
                if (closestTarget != null) target = closestTarget;
                if (actor.isControllable()) selectionCircle.target = actor.transform;
            }
            else
            {
                var possibleTargets = BattleManager.instance.EnemyActors
                    .Where(actor => actor.state.CurrentState != actor.state.deathState)
                    .ToList();

                var closestTarget = possibleTargets
                    .OrderBy(actor => actor.target.DistanceToClosestEnemy)
                    .FirstOrDefault();

                if (closestTarget != null)
                {
                    float currentTargetDistance = target.target.DistanceToClosestEnemy;
                    float closestTargetDistance = closestTarget.target.DistanceToClosestEnemy;

                    // Switch to the closer target if it's significantly closer
                    if (closestTargetDistance < currentTargetDistance)
                    {
                        target = closestTarget;
                    }
                }
                if (actor.isControllable()) selectionCircle.target = target.transform;

            }
            //if it's has a target but another enemy is even closer
        }
    }
}