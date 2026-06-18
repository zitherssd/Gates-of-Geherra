using System.Collections.Generic;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Manager;
using UnityEngine;

namespace Assets.Scripts.Game
{
    /// <summary>
    /// Entry point for a dedicated arena scene — the battle-scene counterpart to
    /// <see cref="GameFlowManager"/> in the rest scene. On load it spawns the persistent player
    /// body, binds it to the run's <see cref="GameSession.PlayerRuntime"/>, registers it with the
    /// scene's <see cref="BattleManager"/>, and starts the battle described by
    /// <see cref="GameSession.PendingBattle"/>.
    ///
    /// Player stats, skills and items live on <see cref="GameSession"/>, so the hand-off needs no
    /// disk round-trip.
    /// </summary>
    public class ArenaBootstrapper : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Player body prefab spawned for this arena and bound to the persistent run.")]
        public Actor playerPrefab;

        [Header("Testing")]
        [Tooltip("Battle to run when this scene is entered directly with no PendingBattle set. Editor testing only.")]
        public BattleDefinition debugBattle;

        private void Start()
        {
            var session = GameSession.Instance;
            var battle = session.PendingBattle != null ? session.PendingBattle : debugBattle;
            if (battle == null)
            {
                Debug.LogWarning($"{nameof(ArenaBootstrapper)}: no PendingBattle and no debugBattle assigned; nothing to start.");
                return;
            }

            if (BattleManager.instance == null)
            {
                Debug.LogError($"{nameof(ArenaBootstrapper)}: no BattleManager in the scene.");
                return;
            }

            var spawnGroup = SpawnGroup.Find(battle.spawnGroup);
            var player = SpawnAndBindPlayer(spawnGroup);
            if (player == null)
            {
                Debug.LogError($"{nameof(ArenaBootstrapper)}: could not create a player body (assign playerPrefab).");
                return;
            }

            // The battle systems track the player through BattleManager.PlayerActors; in an arena
            // the spawned body is the only player actor.
            if (BattleManager.instance.PlayerActors == null)
                BattleManager.instance.PlayerActors = new List<Actor>();
            BattleManager.instance.PlayerActors.Clear();
            BattleManager.instance.PlayerActors.Add(player);

            // Build this scene's action buttons from the run's persisted layout. ActionButtonBattle
            // resolves its player via BattleManager.PlayerActors[0] (set just above), so the buttons
            // drive the freshly-bound body. Start them disabled like the rest scene does;
            // BattleStartState re-enables them when the fight begins.
            if (UIManager.instance != null)
            {
                UIManager.instance.InitializePlayerActionButtonPrefabs(player.Runtime.actions, session.Loadout);
                UIManager.instance.DisableBattleSkills();
            }

            session.PendingBattle = null;
            BattleManager.instance.enabled = true;
            BattleManager.instance.Enter(battle, null);
        }

        private Actor SpawnAndBindPlayer(SpawnGroup spawnGroup)
        {
            if (playerPrefab == null) return null;

            var pos = transform.position;
            var rot = Quaternion.identity;
            if (spawnGroup != null && spawnGroup.playerSpawn != null)
            {
                pos = spawnGroup.playerSpawn.position;
                rot = spawnGroup.playerSpawn.rotation;
            }

            var player = Instantiate(playerPrefab, pos, rot);
            player.Bind(GameSession.Instance.PlayerRuntime);
            GameSession.Instance.NotifyPlayerSpawned(player);
            return player;
        }
    }
}
