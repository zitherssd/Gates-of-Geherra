using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Actions;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Crawler;
using Assets.Scripts.Game;
using Assets.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Assets.Scripts.Battle.Actions.BaseAction;

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
        [SerializeField] private GameObject SpawnerParent;

        private Queue<Actor.Actor> turnQueue = new Queue<Actor.Actor>();

        private bool repeatTurn;
        private uint currentTurn;


        private void Awake()
        {
            if (instance == null) instance = this;

            battleStateMachine = new BattleStateMachine(this);
        }





        public void Enter(BattleDefinition battleDefinition)
        {
            //Should assume that screen is black/fadeimage is active
            var player = GameFlowManager.instance.playerActor;
            player.transform.position = battleDefinition.playerStartPosition;
            player.ActorData.Refresh();
            SpawnEnemies(battleDefinition);
            UIManager.instance.Fade(false, null);
            CameraManager.instance.ResetForNewBattle();
            UIManager.instance.InitializePlayerActionButtonPrefabs(player.ActorData.actions);
            StartCoroutine(WaitForSeconds(2f, () =>
            {
                SoundManager.instance.PlayMusic(null);
                //UIManager
                //.instance.InitializePlayerActionButtonPrefabs(PlayerActors[0].ActorData.actions);
                //UIManager.instance.MoveActionsToBattleActionContainers();
                battleStateMachine.Initialize(battleStateMachine.startState); // Start the battle state machine
            }));

            // Wait 2 seconds while fadeout finishes and camera moves
            // Going strike
            // Initialize battle state machine
        }

        private void SpawnEnemies(BattleDefinition battleDefinition)
        {
            foreach (var enemy in EnemyActors)
            {
                Destroy(enemy.gameObject);
            }
            EnemyActors.Clear();

            foreach (var enemy in battleDefinition.enemyActors)
            {
                var clone = Instantiate(enemy);
                var randomSpawner = GetRandomChild(SpawnerParent);
                var enemyGameObject = Instantiate(enemyPrefab, randomSpawner.position, Quaternion.identity);
                var enemyActor = enemyGameObject.GetComponent<Actor.Actor>();
                enemyActor.ActorData = clone;
                enemyActor.Init();
                EnemyActors.Add(enemyActor);
            }
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



        public void SetupBattleWithEnemies(List<ActorData> newEnemies) //Floor 1,2 etc setup
        {
            foreach (var enemy in EnemyActors)
            {
                Destroy(enemy.gameObject);
            }
            EnemyActors.Clear();

            foreach (var enemy in newEnemies)
            {
                var clone = Instantiate(enemy);
                var randomSpawner = GetRandomChild(SpawnerParent);
                var enemyGameObject = Instantiate(enemyPrefab, randomSpawner.position, Quaternion.identity);
                var enemyActor = enemyGameObject.GetComponent<Actor.Actor>();
                enemyActor.ActorData = clone;
                enemyActor.Init();
                EnemyActors.Add(enemyActor);
            }


            PlayerActors[0].ActorData.Refresh();

            UIManager.instance.EnableBattleSkills();
            UIManager.instance.Fade(false, () =>
            {
                battleStateMachine.TransitionTo(battleStateMachine.startState); // Start the battle state machine
            });
        }

        public IEnumerator WaitForSeconds(float seconds, Action onFinishedWaiting)
        {
            yield return new WaitForSeconds(seconds);
            onFinishedWaiting();
        }

        public static Transform GetRandomChild(GameObject list)
        {
            // Make sure the list is not empty
            if (list.transform.childCount == 0)
                throw new InvalidOperationException("Cannot retrieve a random element from an empty list.");

            return list.transform.GetChild(UnityEngine.Random.Range(0, list.transform.childCount));
            // Return the element at the random index
        }
    }
    public enum STATE { READY, WAITING }
}
