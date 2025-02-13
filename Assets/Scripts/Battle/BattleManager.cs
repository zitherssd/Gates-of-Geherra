using Assets.Scripts.Actions;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.BaseAction;
using static UnityEngine.EventSystems.EventTrigger;

namespace Assets
{

    public class BattleManager : MonoBehaviour
    {
        public GameObject enemyPrefab;
        public Action<uint> OnNewTurn;
        public static BattleManager instance = null;
        [SerializeField] public List<Actor> PlayerActors;
        [SerializeField] public List<Actor> EnemyActors;
        public STATE state = STATE.READY;

        private Queue<Actor> turnQueue = new Queue<Actor>();
        private bool repeatTurn;
        private uint currentTurn;
        private void Awake()
        {
            if (instance == null) instance = this;

            BaseReaction.NoReaction = ScriptableObject.CreateInstance<BaseReaction>();
            BaseReaction.NoReaction.Name = "Nothing";
            MoveAction.instance = ScriptableObject.CreateInstance<MoveAction>();
            MoveAction.instance.Name = "Move";
            MoveAction.instance.Tags = new List<TAG>();
        }
        void Start()
        {
            Application.targetFrameRate = 60;
            Physics.gravity = new Vector3(0, -6f, 0);
            StartCoroutine(UIManager.instance.TypeTextMiddleLetterByLetter($"{PlayerActors[0].ActorData.Name} vs {EnemyActors[0].ActorData.Name}", () =>
            {
                SoundManager.instance.PlayMusic(null);
                StartCoroutine(UIManager.instance.FadeMiddleText(1));
                UIManager.instance.Fade(false, () =>
                {



                    StartCoroutine(WaitForSeconds(2f, () =>
                    {
                        StartBattle();
                    }));
                });
            }));
        }

        //Start floor 1 
        void StartBattle()
        {
            foreach (var actor in PlayerActors.Concat(EnemyActors))
            {
                actor.Initialize(actor.ActorData);
                actor.state.TransitionToIdle();
            }
            UIManager.instance.DrawActions(PlayerActors[0].ActorData.actions);
            UIManager.instance.GainMeter(4f);
        }

        public void SetupBattleWithEnemies(List<ActorData> newEnemies) //Floor 1,2 etc setup
        {
            foreach(var enemy in EnemyActors)
            {
                Destroy(enemy.gameObject);
            }
            EnemyActors.Clear();

            foreach (var enemy in newEnemies)
            {
                var enemyGameObject = Instantiate(enemyPrefab, new Vector3(10, 0, -3), Quaternion.identity);
                var enemyActor = enemyGameObject.GetComponent<Actor>();
                enemyActor.Initialize(enemy);
                EnemyActors.Add(enemyActor);
            }

            PlayerActors[0].ActorData.Refresh();
            CameraManager.instance.ResetForNewBattle();
            UIManager.instance.Fade(false, () =>
            {
                foreach (var actor in PlayerActors.Concat(EnemyActors))
                {
                    actor.state.TransitionTo(actor.state.idleState);
                }
            });
        }

        IEnumerator WaitForSeconds(float seconds, Action onFinishedWaiting)
        {
            yield return new WaitForSeconds(seconds);
            onFinishedWaiting();
        }

        public void End()
        {
            if (PlayerActors.TrueForAll(actor => actor.state.CurrentState == actor.state.deathState))
            {
                EnemyActors[0].PlayAnimation("Victory");
                UIManager.GetInstance().ChangeStatus("YOU LOSE");
                SoundManager.instance.PlaySingle(SoundManager.instance.GetAudioClipByName("Curse2"));
            }
            if (EnemyActors.TrueForAll(actor => actor.state.CurrentState == actor.state.deathState))
            {
                PlayerActors[0].state.TransitionTo(PlayerActors[0].state.blockState);
                PlayerActors[0].PlayAnimation("Victory");
                UIManager.GetInstance().ChangeStatus("YOU WIN!!");
                UIManager.instance.HideUI();
                FloorManager.GetInstance().ProgressToNextFloor();
            }
        }
    }

    public enum STATE { READY, WAITING }
}
