using Assets.Scripts.Actions;
using Assets.Scripts.Battle.Actions;
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


        public enum State { WaitingForPlayer, Busy }
        public State state;
        [SerializeField] public List<BaseActorBattler> PlayerActors;
        [SerializeField] public List<BaseActorBattler> EnemyActors;
        public Queue<BaseActorBattler> turnQueue = new Queue<BaseActorBattler>();
        private BaseActorBattler activeBattler;
        private bool repeatTurn;
        private UIManager uiManager;
        private uint currentTurn;

        private void Awake()
        {
            if (instance == null) instance = this;

            BaseReaction.NoReaction = ScriptableObject.CreateInstance<BaseReaction>();
            BaseReaction.NoReaction.Name = "Nothing";
            BaseReaction.NoReaction.knockbackModifier = 1;
            BaseReaction.NoReaction.damageModifier = 1;
            BaseReaction.NoReaction.postureModifier = 1;
            MoveSkill.instance = ScriptableObject.CreateInstance<MoveSkill>();
            MoveSkill.instance.Name = "Move";

        }
        void Start()
        {
            uiManager = UIManager.GetInstance();
            StartCoroutine(uiManager.TypeTextMiddleLetterByLetter($"{PlayerActors[0].GetBaseActor().Name} vs {EnemyActors[0].GetBaseActor().Name}", () =>
            {
                SoundManager.instance.PlayMusic(null);
                StartCoroutine(uiManager.FadeMiddleText(1));
                StartCoroutine(uiManager.Fade(false, () =>
                {
                    StartCoroutine(WaitForSeconds(0.5f, () =>
                    {
                        SetupBattle();
                        SwitchToNextTurn();
                    }));
                }));
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
            List<BaseActorBattler> allActors = new List<BaseActorBattler>();
            allActors.AddRange(PlayerActors); allActors.AddRange(EnemyActors);
            allActors.OrderBy(x => x.Actor.AGI);

            foreach (var actor in allActors)
            {
                actor.GetBaseActor().Reset();
                actor.Initialize();
                turnQueue.Enqueue(actor);
            }
        }

        public void SetupBattleWithEnemy(Actor enemy)
        {
            //EnemyActors[0].SetBaseActor(enemy);
            //EnemyActors[0].GetBaseActor().Reset();
            //EnemyActors[0].PlayAnimation("Idle");
            //PlayerActors[0].PlayAnimation("Idle");
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

        private void SetActiveCharacterBattle(BaseActorBattler battler)
        {
            if (activeBattler != null)
            {
                //HideCircle?
            }

            activeBattler = battler;
            activeBattler.ShowSelectionCircle();
        }

        public bool TestBattleOver()
        {
            if (PlayerActors.TrueForAll(actor => actor.GetBaseActor().GetCurrentHP() == 0))
            {
                return true;
            }
            if (EnemyActors.TrueForAll(actor => actor.GetBaseActor().GetCurrentHP() == 0))
            {
                return true;
            }
            return false;
        }



        public void RepeatTurn()
        {
            repeatTurn = true;
        }

        public BaseActorBattler GetActiveActor()
        {
            return activeBattler;
        }

        IEnumerator WaitForSeconds(float seconds, Action onFinishedWaiting)
        {
            yield return new WaitForSeconds(seconds);
            onFinishedWaiting();
        }

        //private void StartPlayerTurn()
        //{
        //    UIManager.GetInstance().ChangeStatus("PLAYER TURN");
        //    SetActiveCharacterBattle(PlayerActors[0]);
        //    InputManager.instance.WaitForTurn(SwitchToNextTurn);
        //}
        //private void StartEnemyTurn()
        //{
        //    UIManager.GetInstance().ChangeStatus("ENEMY TURN");
        //    SetActiveCharacterBattle(EnemyActors[0]);

        //    // Enemy Ai here I guess
        //    var chosenSkill = EnemyActors[0].ChooseValidSkillInRangeOrReturnNull();
        //    if (chosenSkill == null)
        //    {
        //        EnemyActors[0].Move((PlayerActors[0].transform.position - EnemyActors[0].transform.position).normalized, () =>
        //        {
        //            var chosenSkill = EnemyActors[0].ChooseValidSkillAtRandomOrReturnNull();
        //            if (chosenSkill == null)
        //                SwitchToNextTurn();
        //            else
        //                EnemyActors[0].EnemyStartCombo(SwitchToNextTurn);
        //        });
        //    }
        //    else
        //        EnemyActors[0].EnemyStartCombo(SwitchToNextTurn);
        //}

        private void SwitchToNextActor()
        {
            Debug.Log("Switching to next actor in turn");

            if (TestBattleOver())
            {
                End();
                return;
            }
            var nextActor = GetNextActorInTurn();
            if (nextActor == null) SwitchToNextTurn();
            else
            {
                SetActiveCharacterBattle(nextActor);
                nextActor.Act(SwitchToNextActor);
            }
        }
        private void SwitchToNextTurn()
        {
            Debug.Log("Switching to next turn");
            if (TestBattleOver())
            {
                End();
                return;
            }

            ProcTurnChangeEffects();

            activeBattler = GetNextActorInTurn();
            SetActiveCharacterBattle(activeBattler);
            activeBattler.Act(SwitchToNextActor);
        }

        private void ProcTurnChangeEffects()
        {
            currentTurn++;
            uiManager.SetTurn(currentTurn);
            turnQueue.Clear();
            List<BaseActorBattler> allActors = new List<BaseActorBattler>();
            allActors.AddRange(PlayerActors); allActors.AddRange(EnemyActors);
            allActors.OrderBy(x => x.Actor.AGI);

            foreach (var actor in allActors)
            {
                actor.ResetFlags();
                turnQueue.Enqueue(actor);
            }
        }

        public void WaitForMovementCompletion(Action onMovementComplete)
        {
            bool allActorsIdle = false;

            while (!allActorsIdle)
            {
                allActorsIdle = true;

                foreach (var actor in PlayerActors.Concat(EnemyActors))
                {
                    if (actor.GetComponent<Rigidbody>().velocity.magnitude > Mathf.Epsilon)
                    {
                        allActorsIdle = false;
                        break; // At least one actor is still moving, exit the loop
                    }
                }
            }

            // All actors are now idle (no movement)
            // Proceed with further actions or logic
            onMovementComplete?.Invoke();

        }

        public BaseActorBattler GetNextActorInTurn()
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
            if (PlayerActors.TrueForAll(actor => actor.GetBaseActor().GetCurrentHP() == 0))
            {
                EnemyActors[0].PlayAnimation("Victory");
                PlayerActors[0].PlayAnimation("Down");
                UIManager.GetInstance().ChangeStatus("YOU LOSE");
                SoundManager.instance.PlaySingle(SoundManager.instance.GetAudioClipByName("Curse2"));
            }
            if (EnemyActors.TrueForAll(actor => actor.GetBaseActor().GetCurrentHP() == 0))
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