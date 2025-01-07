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

namespace Assets
{

    public class BattleManager : MonoBehaviour
    {
        public Action<uint> OnNewTurn;
        public static BattleManager instance = null;
        [SerializeField] public List<Actor> PlayerActors;
        [SerializeField] public List<Actor> EnemyActors;
        public STATE state = STATE.READY;

        private Queue<Actor> turnQueue = new Queue<Actor>();
        private Actor activeBattler;
        private bool repeatTurn;
        private uint currentTurn;
        private bool isHitstopActive = false;
        private float originalTimeScale = 1.0f;
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

        //Starts a battle with current PlayerActors and EnemyActors
        void StartBattle()
        {
            foreach (var actor in PlayerActors.Concat(EnemyActors))
            {
                //actor.ActorData.Reset();
                actor.state.TransitionTo(actor.state.idleState);
            }
            UIManager.instance.DrawActions(PlayerActors[0].ActorData.actions);
            //UIManager.instance.GainMeter(2f);
            Debug.Log("Gained meter!");
            //SwitchToNextTurn();
        }



        public void SetupBattleWithEnemies(List<ActorData> enemy) //Floor 1,2 etc setup
        {
            UIManager.instance.Fade(false, () =>
            {

                EnemyActors[0].ActorData = enemy[0];
                EnemyActors[0].ActorData.Reset();
                EnemyActors[0].transform.Find("UI Elements").GetComponent<BarsHandler>().Reset();
                PlayerActors[0].ActorData.Refresh();
                CameraManager.instance.ResetForNewBattle();
                foreach (var actor in PlayerActors.Concat(EnemyActors))
                {
                    actor.state.TransitionTo(actor.state.idleState);
                }
            });

            //PlayerActors[0].GetBaseActor().currentPosture = PlayerActors[0].GetBaseActor().basePosture;

            //uiManager = UIManager.GetInstance();
            //StartCoroutine(uiManager.TypeTextMiddleLetterByLetter($"{PlayerActors[0].GetBaseActor().Name} vs {EnemyActors[0].GetBaseActor().Name}", () =>
            //{
            //    StartCoroutine(uiManager.FadeMiddleText(1));
            //    StartCoroutine(WaitForSeconds(0.5f, () =>
            //    {
            //        PlayerActors[0].GetBaseActor().Initialize();

            //        if (PlayerActors[0].GetBaseActor().AGI >= EnemyActors[0].GetBaseActor().AGI)
            //            StartPlayerTurn();
            //        else
            //            StartEnemyTurn();
            //    }));
            //}));
        }

        public Actor GetActiveActor()
        {
            return activeBattler;
        }

        IEnumerator WaitForSeconds(float seconds, Action onFinishedWaiting)
        {
            yield return new WaitForSeconds(seconds);
            onFinishedWaiting();
        }

        public void End()
        {
            if (PlayerActors.TrueForAll(actor => actor.ActorData.GetCurrentHP() == 0))
            {
                EnemyActors[0].PlayAnimation("Victory");
                UIManager.GetInstance().ChangeStatus("YOU LOSE");
                SoundManager.instance.PlaySingle(SoundManager.instance.GetAudioClipByName("Curse2"));
            }
            if (EnemyActors.TrueForAll(actor => actor.ActorData.GetCurrentHP() == 0))
            {
                PlayerActors[0].state.TransitionTo(PlayerActors[0].state.blockState);
                PlayerActors[0].PlayAnimation("Victory");
                //SoundManager.instance.PlaySingle(SoundManager.instance.GetAudioClipByName("Like"));
                UIManager.GetInstance().ChangeStatus("YOU WIN!!");
                UIManager.instance.HideUI();
                FloorManager.GetInstance().ProgressToNextFloor();
            }
        }
    }

    public enum STATE { READY, WAITING }
}
