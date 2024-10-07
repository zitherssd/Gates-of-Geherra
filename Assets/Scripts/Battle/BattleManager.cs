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
        private UIManager uiManager;
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
            uiManager = UIManager.GetInstance();
            StartCoroutine(uiManager.TypeTextMiddleLetterByLetter($"{PlayerActors[0].ActorData.Name} vs {EnemyActors[0].ActorData.Name}", () =>
            {
                SoundManager.instance.PlayMusic(null);
                StartCoroutine(uiManager.FadeMiddleText(1));
                uiManager.Fade(false, () =>
                {
                    StartCoroutine(WaitForSeconds(0.1f, () =>
                    {
                        StartBattle();
                    }));
                });
            }));
        }

        private void Update()
        {
            switch (state)
            {
                case STATE.READY:

                    break;
                case STATE.WAITING:
                    if ((PlayerActors.Concat(EnemyActors).ToList().TrueForAll(x => x.state.CurrentState == x.state.idleState)))
                    {
                        state = STATE.READY;
                    }
                    break;
            }
        }

        public void ApplyHitstop(float duration)
        {
            if (!isHitstopActive)
            {
                StartCoroutine(HitstopCoroutine(duration));
            }
        }

        private IEnumerator HitstopCoroutine(float duration)
        {
            isHitstopActive = true;
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0.0f;

            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = originalTimeScale;
            isHitstopActive = false;
        }

        //Starts a battle with current PlayerActors and EnemyActors
        void StartBattle()
        {
            currentTurn = 0;
            uiManager.SetTurn(currentTurn);
            turnQueue.Clear();
            foreach(var actor in PlayerActors.Concat(EnemyActors))
            {
                actor.ActorData.Reset();
                actor.state.TransitionTo(actor.state.idleState);
            }
            EnqueAll();
            Time.timeScale = 0f;
            //SwitchToNextTurn();
        }

        private void EnqueAll()
        {
            var allActors = PlayerActors.Concat(EnemyActors).ToList().OrderByDescending(x => x.ActorData.AGI);

            foreach (var actor in allActors)
            {
                turnQueue.Enqueue(actor);
            }
        }

        public void SetupBattleWithEnemy(ActorData enemy) //Floor 1,2 etc setup
        {
            uiManager.Fade(false, () =>
            {
                EnemyActors[0].ActorData = enemy;
                EnemyActors[0].ActorData.Reset();
                EnemyActors[0].transform.Find("UI Elements").GetComponent<BarsHandler>().Reset();
                EnemyActors[0].PlayAnimation("Idle");
                PlayerActors[0].PlayAnimation("Idle");
                PlayerActors[0].ActorData.Refresh();
                currentTurn = 0;
                uiManager.SetTurn(currentTurn);
                turnQueue.Clear();
                CameraManager.instance.ResetForNewBattle();
                EnqueAll();
                SwitchToNextTurn();
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

        private bool TestBattleOver()
        {
            if (PlayerActors.TrueForAll(actor => actor.ActorData.GetCurrentHP() == 0))
            {
                return true;
            }
            if (EnemyActors.TrueForAll(actor => actor.ActorData.GetCurrentHP() == 0))
            {
                return true;
            }
            return false;
        }

        //Legacy
        public Actor GetActiveActor()
        {
            return activeBattler;
        }

        IEnumerator WaitForSeconds(float seconds, Action onFinishedWaiting)
        {
            yield return new WaitForSeconds(seconds);
            onFinishedWaiting();
        }
        
        private void SwitchToNextActorInTurn()
        {
            if (TestBattleOver())
            {
                End();
                return;
            }


            StartCoroutine(WaitForMovementCompletion(() =>
            {
                var nextActor = GetNextActorInTurn();
                if (nextActor == null)
                {
                    ProcTurnChangeEffects();
                    SwitchToNextTurn();
                }
                else
                {
                    nextActor.Act(SwitchToNextActorInTurn);
                }
            }));
        }
        private void SwitchToNextTurn()
        {
            Debug.Log($"-------------TURN {currentTurn}------------");

            if (TestBattleOver())
            {
                End();
                return;
            }

            StartCoroutine(WaitForMovementCompletion(() =>
            { 
                activeBattler = GetNextActorInTurn();
                activeBattler.Act(SwitchToNextActorInTurn);
            }));
           

            //// Delay turn switch until actors no longer have the Midair state
            //StartCoroutine(WaitForMovementCompletion(() =>
            //{
            //    ProcTurnChangeEffects();
            //    activeBattler = GetNextActorInTurn();
            //    activeBattler.Act(SwitchToNextActorInTurn);
            //}));
        }
        private void ProcTurnChangeEffects()
        {
            currentTurn++;
            uiManager.SetTurn(currentTurn);
            OnNewTurn?.Invoke(currentTurn);
            EnqueAll();
            foreach(var actor in PlayerActors.Concat(EnemyActors))
            {
                actor.ProcNextTurnEffects();
            }
        }
        private IEnumerator WaitForMovementCompletion(Action onMovementComplete)
        {
            var allActors = PlayerActors.Concat(EnemyActors);

            while (!allActors.All(actor => actor.state.CurrentState == actor.state.idleState || actor.state.CurrentState == actor.state.blockState))
            {
                yield return null; // Wait for the next frame
            }

            // Midair state is removed for all actors, proceed with turn switch
            onMovementComplete?.Invoke();
        }
        private Actor GetNextActorInTurn()
        {
            if (turnQueue.Count > 0)
            {
                return turnQueue.Dequeue();
            }
            else
                return null;
        }
        private void End()
        {
            if (PlayerActors.TrueForAll(actor => actor.ActorData.GetCurrentHP() == 0))
            {
                EnemyActors[0].PlayAnimation("Victory");
                PlayerActors[0].PlayAnimation("Down");
                UIManager.GetInstance().ChangeStatus("YOU LOSE");
                SoundManager.instance.PlaySingle(SoundManager.instance.GetAudioClipByName("Curse2"));
            }
            if (EnemyActors.TrueForAll(actor => actor.ActorData.GetCurrentHP() == 0))
            {
                EnemyActors[0].PlayAnimation("Down");
                PlayerActors[0].PlayAnimation("Victory");
                //SoundManager.instance.PlaySingle(SoundManager.instance.GetAudioClipByName("Like"));
                UIManager.GetInstance().ChangeStatus("YOU WIN!!");
                FloorManager.GetInstance().ProgressToNextFloor();
            }
        }
    }

    public enum STATE { READY, WAITING }
}
