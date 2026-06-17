using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Game;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Manager
{

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager instance = null;

        public GameObject enemyPrefab;
        public Action<uint> OnNewTurn;
        public BattleStateMachine battleStateMachine;
        [SerializeField] public List<Actor.Actor> PlayerActors;
        [SerializeField] public List<Actor.Actor> EnemyActors;
        public event Action OnBattleEnd;



        private Queue<Actor.Actor> turnQueue = new Queue<Actor.Actor>();

        private bool repeatTurn;
        private uint currentTurn;
        private BattleDefinition battleDefinition;

        private void Awake()
        {
            if (instance == null) instance = this;

            battleStateMachine = new BattleStateMachine(this);
        }



        internal BattleDefinition GetCurrentBattleDefinition()
        {
            if (battleDefinition == null)
                throw new Exception("No current battle definition set in BattleManager");
            else
                return battleDefinition;
        }

        /// <summary>
        /// The player body for the current battle: the first registered PlayerActor, falling back
        /// to the RestScene's GameFlowManager player. Lets battle code run in dedicated arena
        /// scenes that have no GameFlowManager.
        /// </summary>
        public Actor.Actor Player
        {
            get
            {
                if (PlayerActors != null && PlayerActors.Count > 0 && PlayerActors[0] != null)
                    return PlayerActors[0];
                return GameFlowManager.instance != null ? GameFlowManager.instance.playerActor : null;
            }
        }

        public void Enter(BattleDefinition battleDefinition, Action onBattleEnd)
        {
            OnBattleEnd = null;
            if (onBattleEnd != null)
                OnBattleEnd += onBattleEnd;

            this.battleDefinition = battleDefinition;
            //Should assume that screen is black/fadeimage is active
            var player = Player;
            var spawnGroup = SpawnGroup.Find(battleDefinition.spawnGroup);
            var level = GameObject.Find("Level_" + battleDefinition.Level); // legacy; null in dedicated arena scenes
            player.transform.position = PlayerSpawnPosition(spawnGroup, level, player.transform.position);
            player.Runtime.Refresh();
            SpawnEnemies(battleDefinition, spawnGroup, level);
            UIManager.instance.Fade(false, null);
            CameraManager.instance.ResetForNewBattle(CameraAnchorPosition(spawnGroup, level));
            StartCoroutine(WaitForSeconds(2f, () =>
            {
                SoundManager.instance.PlayMusic(null);
                battleStateMachine.Initialize(battleStateMachine.startState); // Start the battle state machine
            }));
        }

        private void SpawnEnemies(BattleDefinition battleDefinition, SpawnGroup spawnGroup, GameObject level)
        {
            foreach (var enemy in EnemyActors)
            {
                Destroy(enemy.gameObject);
            }
            EnemyActors.Clear();

            // Legacy spawners: first child of the level holds player (child 0) + enemy (child 1..) markers.
            var legacySpawners = level != null ? level.transform.GetChild(0) : null;

            for (int i = 0; i < battleDefinition.enemyActors.Count(); i++)
            {
                var enemy = battleDefinition.enemyActors[i];
                var position = EnemySpawnPosition(spawnGroup, legacySpawners, i);

                var enemyGameObject = Instantiate(enemyPrefab, position, Quaternion.identity);
                var enemyActor = enemyGameObject.GetComponent<Actor.Actor>();
                enemyActor.SetDefinition(enemy);
                enemyActor.Init();
                enemyActor.Spawn();
                EnemyActors.Add(enemyActor);
            }
        }

        // Prefer the scene's SpawnGroup; fall back to the legacy in-scene "Level_<n>" markers.
        private static Vector3 PlayerSpawnPosition(SpawnGroup spawnGroup, GameObject level, Vector3 fallback)
        {
            if (spawnGroup != null && spawnGroup.playerSpawn != null)
                return spawnGroup.playerSpawn.position;
            if (level != null)
                return level.transform.GetChild(0).GetChild(0).position;
            return fallback;
        }

        private static Vector3 EnemySpawnPosition(SpawnGroup spawnGroup, Transform legacySpawners, int index)
        {
            if (spawnGroup != null && spawnGroup.enemySpawns != null && spawnGroup.enemySpawns.Count > 0)
            {
                var t = spawnGroup.enemySpawns[index % spawnGroup.enemySpawns.Count];
                if (t != null) return t.position;
            }
            if (legacySpawners != null)
                return legacySpawners.GetChild(index + 1).position;
            return Vector3.zero;
        }

        private static Vector3 CameraAnchorPosition(SpawnGroup spawnGroup, GameObject level)
        {
            if (spawnGroup != null && spawnGroup.cameraAnchor != null)
                return spawnGroup.cameraAnchor.position;
            if (level != null)
                return level.transform.Find("CameraTransformPosition").position;
            return Vector3.zero;
        }

        void Start()
        {
            Physics.gravity = new Vector3(0, -6f, 0);

            // Camera pan wait


        }

        public void Update()
        {
            // Update the battle state machine
            battleStateMachine.Update();
        }

        public void TriggerBattleEnd()
        {
            OnBattleEnd?.Invoke();
        }

        public IEnumerator WaitForSeconds(float seconds, Action onFinishedWaiting)
        {
            yield return new WaitForSeconds(seconds);
            onFinishedWaiting();
        }
    }
    public enum STATE { READY, WAITING }
}
