using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Actions;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Crawler;
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
        void Start()
        {
            Physics.gravity = new Vector3(0, -6f, 0);

            // Initialize player only for existing prefabs
            foreach (var actor in PlayerActors.Concat(EnemyActors))
            {
                actor.Initialize(actor.ActorData);
            }

            // Camera pan wait
            StartCoroutine(WaitForSeconds(2f, () =>
            {
                SoundManager.instance.PlayMusic(null);
                battleStateMachine.Initialize(battleStateMachine.startState); // Start the battle state machine
            }));



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
                enemyActor.Initialize(clone);
                EnemyActors.Add(enemyActor);
            }

            UIManager.instance.DrawActions(PlayerActors[0].ActorData.actions);
            PlayerActors[0].ActorData.Refresh();
            CameraManager.instance.ResetForNewBattle();
            
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

        public void End()
        {
            if (PlayerActors.TrueForAll(actor => !actor.state.IsAlive()))
            {
                EnemyActors[0].PlayAnimation("Victory");
                UIManager.GetInstance().ChangeStatus("Defeat");
                //SoundManager.instance.PlaySingle(SoundManager.instance.GetAudioClipByName("Curse2"));
                StartCoroutine(WaitForSeconds(2f, () => { SceneManager.LoadScene("TitleScene"); }));
            }
            if (EnemyActors.TrueForAll(actor => !actor.state.IsAlive()))
            {
                PlayerActors[0].state.TransitionTo(PlayerActors[0].state.blockState);
                PlayerActors[0].PlayAnimation("Victory");
                UIManager.instance.HideUI();
               
            }
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
