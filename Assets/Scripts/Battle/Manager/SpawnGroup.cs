using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Manager
{
    /// <summary>
    /// Identifies a set of spawn points within an arena. A single arena can contain several
    /// <see cref="SpawnGroup"/>s so different battles can start in different parts of it.
    /// </summary>
    public enum SpawnGroupId
    {
        Default = 0,
        North,
        South,
        East,
        West,
        Center,
    }

    /// <summary>
    /// Scene-placed marker describing where the player and enemies spawn for a battle, plus an
    /// optional camera anchor. Battles pick a group via <see cref="BattleDefinition.spawnGroup"/>.
    /// Keeping spawn locations in the scene (not on the BattleDefinition asset) lets the same
    /// battle definition work in any arena and removes hard-coded world coordinates.
    /// </summary>
    public class SpawnGroup : MonoBehaviour
    {
        public SpawnGroupId id = SpawnGroupId.Default;

        [Tooltip("Where the player body is placed when this group is used.")]
        public Transform playerSpawn;

        [Tooltip("Spawn points for enemies. Reused cyclically if there are more enemies than points.")]
        public List<Transform> enemySpawns = new List<Transform>();

        [Tooltip("Optional camera framing anchor for the battle. Falls back to legacy logic if unset.")]
        public Transform cameraAnchor;

        /// <summary>
        /// Find an active <see cref="SpawnGroup"/> with the given id in the loaded scene(s).
        /// Returns null if none is placed (callers fall back to legacy positioning).
        /// </summary>
        public static SpawnGroup Find(SpawnGroupId id)
        {
            var groups = Object.FindObjectsOfType<SpawnGroup>();
            foreach (var group in groups)
            {
                if (group.id == id)
                    return group;
            }
            return null;
        }
    }
}
