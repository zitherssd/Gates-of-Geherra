using System.Collections.Generic;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor;
using UnityEngine;

namespace Assets.Scripts.Battle.Manager
{
    [CreateAssetMenu(fileName = "Battle", menuName = "ScriptableObjects/Battle")]
    public class BattleDefinition : ScriptableObject
    {
        public List<ActorData> enemyActors;
        public List<Vector3> enemyStartPosition;
        public Vector3 playerStartPosition;
        public RewardPool RewardPool;
        public List<RewardItemsPool> RewardItemsPools;

    }

}
