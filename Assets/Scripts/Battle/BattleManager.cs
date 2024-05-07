using Assets.Scripts.Actions;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.States;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager instance = null;
        [SerializeField] public List<Actor> PlayerActors;
        [SerializeField] public List<Actor> EnemyActors;

        private Queue<Actor> turnQueue = new Queue<Actor>();
        private Actor activeBattler;
        private bool repeatTurn;
        private UIManager uiManager;
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
            uiManager = UIManager.GetInstance();
            StartCoroutine(uiManager.TypeTextMiddleLetterByLetter($"{PlayerActors[0].ActorData.Name} vs {EnemyActors[0].ActorData.Name}", () =>
            {
                SoundManager.instance.PlayMusic(null);
                StartCoroutine(uiManager.FadeMiddleText(1));
                uiManager.Fade(false, () =>
                {
                    StartCoroutine(WaitForSeconds(0.1f, () =>
                    {
                        SetupBattle();
                        SwitchToNextTurn();
                    }));
                });
            }));
        }

        void SetupBattle() //Turn 0 Setup
        {
            //Assing the enemies. This is mostly done in the inspctor.
            //Units are already spawned.
            //chose dialogue
            Debug.Log("Setting up battle...");
            currentTurn = 0;
            uiManager.SetTurn(currentTurn);
            turnQueue.Clear();
            List<Actor> allActors = new List<Actor>();
            allActors.AddRange(PlayerActors); allActors.AddRange(EnemyActors);
            allActors.OrderBy(x => x.ActorData.AGI);

            foreach (var actor in allActors)
            {
                actor.ActorData.Reset();
                turnQueue.Enqueue(actor);
            }
        }

        public void SetupBattleWithEnemy(ActorData enemy) //Floor 1,2 etc setup
        {
            uiManager.Fade(false, () =>
            {
                EnemyActors[0].ActorData = enemy;
                EnemyActors[0].ActorData.Reset();
                EnemyActors[0].PlayAnimation("Idle");
                PlayerActors[0].PlayAnimation("Idle");
                PlayerActors[0].ActorData.Refresh();
                currentTurn = 0;
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

        private void SetActiveCharacterBattle(Actor battler)
        {
            if (activeBattler != null)
            {
                //HideCircle?
            }

            activeBattler = battler;
            activeBattler.ShowSelectionCircle();
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

        public Actor GetActiveActor()
        {
            return activeBattler;
        }

        IEnumerator WaitForSeconds(float seconds, Action onFinishedWaiting)
        {
            yield return new WaitForSeconds(seconds);
            onFinishedWaiting();
        }
        private void SwitchToNextActor()
        {
            if (TestBattleOver())
            {
                End();
                return;
            }
            StartCoroutine(WaitForMovementCompletion(() =>
            {
                var nextActor = GetNextActorInTurn();
                if (nextActor == null) SwitchToNextTurn();
                else
                {
                    SetActiveCharacterBattle(nextActor);
                    nextActor.Act(SwitchToNextActor);
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

            // Delay turn switch until actors no longer have the Midair state
            StartCoroutine(WaitForMovementCompletion(() =>
            {
                ProcTurnChangeEffects();
                activeBattler = GetNextActorInTurn();
                SetActiveCharacterBattle(activeBattler);
                activeBattler.Act(SwitchToNextActor);
            }));
        }
        private void ProcTurnChangeEffects()
        {
            currentTurn++;
            uiManager.SetTurn(currentTurn);
            turnQueue.Clear();
            List<Actor> allActors = new List<Actor>();
            allActors.AddRange(PlayerActors); allActors.AddRange(EnemyActors);
            allActors = allActors.OrderByDescending(x => x.ActorData.AGI).ToList();

            foreach (var actor in allActors)
            {
                actor.ProcNextTurnEffects();
                turnQueue.Enqueue(actor);
            }
        }
        private IEnumerator WaitForMovementCompletion(Action onMovementComplete)
        {
            var allActors = PlayerActors.Concat(EnemyActors);

            while (allActors.Any(actor => actor.activeStates.OfType<Midair>().Any()))
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
}