using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.Target
{
    public class TargetingManager
    {
        private readonly Actor actor;

        public TargetingManager(Actor actor)
        {
            this.actor = actor;
        }

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
    }
}