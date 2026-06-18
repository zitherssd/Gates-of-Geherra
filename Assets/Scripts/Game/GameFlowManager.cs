using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Game
{
    public class GameFlowManager : MonoBehaviour
    {
        public static GameFlowManager instance;

        [Header("Player body for this scene")]
        [Tooltip("Optional pre-placed player body. If null, one is spawned from playerPrefab.")]
        public Actor playerActor;
        [Tooltip("Optional. Spawned at playerSpawnPoint when no pre-placed playerActor is set.")]
        public Actor playerPrefab;
        [Tooltip("Where the player body is spawned / repositioned for this scene.")]
        public Transform playerSpawnPoint;

        public RestAreaManager restAreaManager;
        public BattleManager battleManager;

        // Run-progress now lives on the persistent GameSession; these delegate so existing
        // callers (SaveManager, TrainingManager, TimeLockManager) keep working unchanged.
        public int trainingsDone
        {
            get => GameSession.Instance.trainingsDone;
            set => GameSession.Instance.trainingsDone = value;
        }
        public int trainingsDoneThisFloor
        {
            get => GameSession.Instance.trainingsDoneThisFloor;
            set => GameSession.Instance.trainingsDoneThisFloor = value;
        }
        public List<TimeLock> timelocks
        {
            get => GameSession.Instance.timelocks;
            set => GameSession.Instance.timelocks = value;
        }


        //Player should be spawned and loaded or created here all other managers assume player exists


        public void Start()
        {
            var session = GameSession.Instance;

            List<ActionSlotSaveData> loadedLoadout = null;
            bool isNewGame = false;

            if (SaveManager.instance == null)
            {
                // No save system present (e.g. a scene opened directly for testing): fresh run.
                if (session.PlayerRuntime == null)
                    session.StartNewRun(null);
                SetupPlayerBody();
                SetMode(GameMode.RestArea);
                UIManager.instance.InitializePlayerActionButtonPrefabs(playerActor.Runtime.actions, session.Loadout);
                UIManager.instance.DisableBattleSkills();
                return;
            }

            if (session.PlayerRuntime != null)
            {
                // Returning from another scene in the same run: the model is already in memory.
                SetupPlayerBody();
            }
            else if (SaveManager.instance.SlotExists(SaveManager.instance.currentSaveSlot))
            {
                var save = SaveManager.instance.LoadFromSlot(SaveManager.instance.currentSaveSlot);
                EnsurePlayerBody();
                save.LoadActor(playerActor);                     // hydrate the body's runtime from disk
                session.AdoptPlayerRuntime(playerActor.Runtime); // session owns it from now on
                loadedLoadout = save.player.actions;
                BindPlayerBody();
            }
            else
            {
                isNewGame = true;
                session.StartNewRun(SaveManager.instance.newGamePlayerName);
                SetupPlayerBody();
            }

            //Now that player is loaded we go to rest
            SetMode(GameMode.RestArea);
            // Prefer the in-memory layout (returning from an arena in the same run); fall back to the
            // layout loaded from disk on a fresh load, or null (default placement) for a new game.
            var layout = session.Loadout ?? loadedLoadout;
            UIManager.instance.InitializePlayerActionButtonPrefabs(playerActor.Runtime.actions, layout);
            UIManager.instance.DisableBattleSkills();

            if (isNewGame)
            {
                SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
            }
        }

        /// <summary>Ensure a player body exists for this scene (spawn from prefab if needed).</summary>
        private void EnsurePlayerBody()
        {
            if (playerActor != null) return;

            if (playerPrefab == null)
            {
                Debug.LogError("GameFlowManager: no playerActor assigned and no playerPrefab to spawn.");
                return;
            }

            Vector3 pos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            Quaternion rot = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;
            playerActor = Instantiate(playerPrefab, pos, rot);
        }

        /// <summary>Ensure the body exists, bind it to the persistent runtime, and announce it.</summary>
        private void SetupPlayerBody()
        {
            EnsurePlayerBody();
            BindPlayerBody();
        }

        private void BindPlayerBody()
        {
            if (playerActor == null) return;

            playerActor.Bind(GameSession.Instance.PlayerRuntime);
            if (playerSpawnPoint != null)
                playerActor.transform.position = playerSpawnPoint.position;

            GameSession.Instance.NotifyPlayerSpawned(playerActor);
        }

        public GameMode Mode { get; private set; }

        public void Awake()
        {
            instance = this;
        }


        public void EnterBattle(BattleDefinition battle, Action onBattleEnd )
        {
            // Dedicated arena: hand off to another scene. The ArenaBootstrapper there reads
            // GameSession.PendingBattle, spawns + binds the player, and starts the battle.
            if (ArenaCatalog.HasScene(battle.arena) &&
                ArenaCatalog.SceneName(battle.arena) != SceneManager.GetActiveScene().name)
            {
                if (onBattleEnd != null)
                    Debug.LogWarning("EnterBattle: onBattleEnd callbacks are not carried across scene loads; ignoring for arena battle.");

                var session = GameSession.Instance;
                session.PendingBattle = battle;
                session.ReturnScene = SceneManager.GetActiveScene().name;
                session.Loadout = UIManager.instance.GetCurrentLoadout(); // preserve button layout across the load
                SceneManager.LoadScene(ArenaCatalog.SceneName(battle.arena));
                return;
            }

            // Legacy in-scene battle (e.g. CaveScene): fight right here.
            Mode = GameMode.Battle;
            restAreaManager.enabled = false;
            TrainingManager.instance.enabled = false;
            battleManager.enabled = true;
            battleManager.Enter(battle, onBattleEnd);
        }

        public void SetMode(GameMode mode)
        {
            Mode = mode;
            battleManager.enabled = false;

            // Rest is the only in-scene mode that still does setup; other transitions are scene loads.
            if (mode == GameMode.RestArea)
            {
                restAreaManager.enabled = true;
                TrainingManager.instance.enabled = true;
                restAreaManager.Enter();
            }
        }
    }
    public enum GameMode
    {
        RestArea,
        Training,
        Exploring,
        Battle,
        StoryBattle,
        Timeout,
    }


}
