using System.Collections.Generic;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor;
using UnityEngine;

namespace Assets.Scripts.Battle.Manager
{
    [CreateAssetMenu(fileName = "Battle", menuName = "ScriptableObjects/Battle")]
    public class BattleDefinition : ScriptableObject
    {
        public List<ActorDefinition> enemyActors;

        [Header("Where this battle takes place")]
        [Tooltip("Arena scene to fight in. None = fight in the current scene (legacy in-scene flow).")]
        public Arena arena = Arena.None;

        [Tooltip("Which SpawnGroup in the arena to use for player/enemy placement.")]
        public SpawnGroupId spawnGroup = SpawnGroupId.Default;

        [Tooltip("Legacy: in-scene level object suffix, used as 'Level_<Level>' when no SpawnGroup is found.")]
        public string Level;

        public RewardPool RewardPool;
        public List<RewardItemsPool> RewardItemsPools;

    }

}
