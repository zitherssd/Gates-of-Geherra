using System;
using System.Collections.Generic;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Save;
using UnityEngine;

namespace Assets.Scripts.Game
{
    /// <summary>
    /// Persistent, data-only source of truth for a single run. Survives scene loads
    /// (DontDestroyOnLoad), so scene-to-scene transitions need no disk round-trip. Disk
    /// persistence still happens only at explicit save points via SaveManager.
    ///
    /// Holds NO scene references (no bodies, managers, Transforms). The player BODY (the Actor
    /// prefab) is spawned per scene and bound to <see cref="PlayerRuntime"/> via Actor.Bind().
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        private static GameSession _instance;

        /// <summary>
        /// The single live session. Auto-creates a persistent instance on first access so
        /// delegating accessors and direct-scene editor testing never hit a null session.
        /// </summary>
        public static GameSession Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(GameSession));
                    _instance = go.AddComponent<GameSession>();
                }
                return _instance;
            }
        }

        /// <summary>True if a session already exists (without creating one).</summary>
        public static bool Exists => _instance != null;

        // --- Persistent run state (data only) ---
        public ActorRuntime PlayerRuntime;
        public int currentFloor;
        public int trainingsDone;
        public int trainingsDoneThisFloor;
        public List<TimeLock> timelocks = new List<TimeLock>();

        /// <summary>Battle to start after the next scene load (rest -> arena hand-off).</summary>
        public BattleDefinition PendingBattle;

        /// <summary>Scene to return to when an arena battle ends (empty = legacy in-scene flow).</summary>
        public string ReturnScene;

        /// <summary>Persisted action-button layout, so the player's arrangement survives scene loads.</summary>
        public List<ActionSlotSaveData> Loadout;

        /// <summary>Raised after the player body is spawned and bound in the current scene.</summary>
        public event Action<Actor> OnPlayerSpawned;

        private const string PlayerTemplatePath = "Actors/MC";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            if (timelocks == null)
                timelocks = new List<TimeLock>();
        }

        /// <summary>
        /// Begin a fresh run: build the player model from the MC template, roll starting stats
        /// and reset run progress. Stats and name are applied last so they take effect (the
        /// body is bound to this runtime and never re-reset to the definition).
        /// </summary>
        public void StartNewRun(string playerName)
        {
            var template = Resources.Load<ActorDefinition>(PlayerTemplatePath);
            PlayerRuntime = new ActorRuntime(template);

            RollStats(PlayerRuntime);
            if (!string.IsNullOrEmpty(playerName))
                PlayerRuntime.Name = playerName;

            currentFloor = 0;
            trainingsDone = 0;
            trainingsDoneThisFloor = 0;
            timelocks = new List<TimeLock>();
            PendingBattle = null;
            ReturnScene = null;
            Loadout = null;
        }

        /// <summary>Adopt a runtime that was just hydrated from a save into a body.</summary>
        public void AdoptPlayerRuntime(ActorRuntime runtime)
        {
            PlayerRuntime = runtime;
        }

        /// <summary>Clear all run state (e.g. on player death / returning to the title screen).</summary>
        public void ClearRun()
        {
            PlayerRuntime = null;
            currentFloor = 0;
            trainingsDone = 0;
            trainingsDoneThisFloor = 0;
            timelocks = new List<TimeLock>();
            PendingBattle = null;
            ReturnScene = null;
            Loadout = null;
        }

        /// <summary>Notify listeners that the player body for the current scene is ready.</summary>
        public void NotifyPlayerSpawned(Actor player)
        {
            OnPlayerSpawned?.Invoke(player);
        }

        private static void RollStats(ActorRuntime runtime)
        {
            runtime.Strength = Roll3d6();
            runtime.Agility = Roll3d6();
            runtime.Mind = Roll3d6();
            runtime.Spirit = Roll3d6();
        }

        private static int Roll3d6()
        {
            return UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7);
        }
    }
}
