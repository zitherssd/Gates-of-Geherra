using Assets.Scripts.Actions;
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
        public enum State { WaitingForPlayer, Busy }
        private static BattleManager instance;
        public State state;
        public static BattleManager GetInstance()
        {
            return instance;
        }
        [SerializeField] public List<BaseActorBattler> PlayerActors;
        [SerializeField] public List<BaseActorBattler> EnemyActors;
        private BaseActorBattler activeBattler;
        private bool repeatTurn;
        private UIManager uiManager;

        private void Awake()
        {
            instance = this;

            BaseReaction.NoReaction = ScriptableObject.CreateInstance<BaseReaction>();
            BaseReaction.NoReaction.Name = "Nothing";
            BaseReaction.NoReaction.knockbackModifier = 1;
            BaseReaction.NoReaction.damageModifier = 1;
            BaseReaction.NoReaction.postureModifier = 1;
        }

        void Start()
        {
            uiManager = UIManager.GetInstance();
            StartCoroutine(uiManager.TypeTextMiddleLetterByLetter($"{PlayerActors[0].GetBaseActor().Name} vs {EnemyActors[0].GetBaseActor().Name}", () =>
            {
                //SoundManager.instance.PlayMusic(null);
                StartCoroutine(uiManager.FadeMiddleText(1));
                StartCoroutine(uiManager.Fade(false, () =>
                {
                    StartCoroutine(WaitForSeconds(0.5f, () =>
                    {
                        SetupBattle();
                        if (PlayerActors[0].GetBaseActor().AGI >= EnemyActors[0].GetBaseActor().AGI)
                            StartPlayerTurn();
                        else
                            StartEnemyTurn();
                    }));
                }));
            }));
        }

        void SetupBattle()
        {
            //Assing the enemies. This is mostly done in the inspctor.
            //Units are already spawned.
            //chose dialogue
            foreach (var actor in PlayerActors)
            {
                actor.GetBaseActor().Reset();
                actor.Initialize();
                actor.GetBaseActor().Controllable = true;
            }
            foreach (var actor in EnemyActors)
            {
                actor.GetBaseActor().Reset();
                actor.Initialize();
                actor.GetBaseActor().Controllable = false;
            }
        }

        public void SetupBattleWithEnemy(Actor enemy)
        {
            EnemyActors[0].SetBaseActor(enemy);
            EnemyActors[0].GetBaseActor().Reset();
            EnemyActors[0].PlayAnimation("Idle");
            PlayerActors[0].PlayAnimation("Idle");
            PlayerActors[0].GetBaseActor().currentPosture = PlayerActors[0].GetBaseActor().basePosture;

            uiManager = UIManager.GetInstance();
            StartCoroutine(uiManager.TypeTextMiddleLetterByLetter($"{PlayerActors[0].GetBaseActor().Name} vs {EnemyActors[0].GetBaseActor().Name}", () =>
            {
                StartCoroutine(uiManager.FadeMiddleText(1));
                StartCoroutine(WaitForSeconds(0.5f, () =>
                {
                    PlayerActors[0].GetBaseActor().Initialize();

                    if (PlayerActors[0].GetBaseActor().AGI >= EnemyActors[0].GetBaseActor().AGI)
                        StartPlayerTurn();
                    else
                        StartEnemyTurn();
                }));
            }));
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

        private void ChooseNextActiveCharacter()
        {
            if (TestBattleOver()) { return; }

            EnemyActors[0].ResetFlags();
            PlayerActors[0].ResetFlags();

            if (!repeatTurn)
            {
                HandleRegularTurn();
            }
            else
            {
                repeatTurn = false;
                HandleRepeatTurn();
            }
        }

        private void HandleRegularTurn()
        {
            if (activeBattler == PlayerActors[0])
            {
                UIManager.GetInstance().ChangeStatus("ENEMY TURN");

                SetActiveCharacterBattle(EnemyActors[0]);
                state = State.Busy;

                StartCoroutine(WaitForSeconds(0.5f, () =>
                {
                    var chosenSkill = EnemyActors[0].ChooseValidSkillAtRandomOrReturnNull();
                    if (chosenSkill == null)
                    {
                        EnemyActors[0].Move((PlayerActors[0].transform.position - EnemyActors[0].transform.position).normalized, () =>
                        {
                            var chosenSkill = EnemyActors[0].ChooseValidSkillAtRandomOrReturnNull();
                            if (chosenSkill == null)
                                ChooseNextActiveCharacter();
                            else
                                EnemyActors[0].UseSkill(chosenSkill, ChooseNextActiveCharacter);
                        });
                    }
                    else
                        EnemyActors[0].UseSkill(chosenSkill, ChooseNextActiveCharacter);
                }));
            }
            else
            {
                UIManager.GetInstance().ChangeStatus("PLAYER TURN");
                SetActiveCharacterBattle(PlayerActors[0]);
                InputManager.instance.WaitForTurn(ChooseNextActiveCharacter);
            }
        }

        private void HandleRepeatTurn()
        {
            SetActiveCharacterBattle(activeBattler);

            if (activeBattler == PlayerActors[0])
            {
                EnemyActors[0].GetBaseActor().currentPosture = EnemyActors[0].GetBaseActor().basePosture;
                InputManager.instance.WaitForTurn(ChooseNextActiveCharacter);
            }
            else
            {
                PlayerActors[0].GetBaseActor().currentPosture = PlayerActors[0].GetBaseActor().basePosture;
                var chosenSkill = EnemyActors[0].ChooseValidSkillAtRandomOrReturnNull();
                if (chosenSkill == null)
                {
                    EnemyActors[0].Move((PlayerActors[0].transform.position - EnemyActors[0].transform.position).normalized, () =>
                    {
                        var chosenSkill = EnemyActors[0].ChooseValidSkillAtRandomOrReturnNull();
                        if (chosenSkill == null)
                            ChooseNextActiveCharacter();
                        else
                            EnemyActors[0].UseSkill(chosenSkill, ChooseNextActiveCharacter);
                    });
                }
                else
                    EnemyActors[0].UseSkill(chosenSkill, ChooseNextActiveCharacter);
            }
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

        private void StartPlayerTurn()
        {
            UIManager.GetInstance().ChangeStatus("PLAYER TURN");
            SetActiveCharacterBattle(PlayerActors[0]);
            InputManager.instance.WaitForTurn(SwitchToNextTurn);
        }
        private void StartEnemyTurn()
        {
            UIManager.GetInstance().ChangeStatus("ENEMY TURN");
            SetActiveCharacterBattle(EnemyActors[0]);

            // Enemy Ai here I guess
            var chosenSkill = EnemyActors[0].ChooseValidSkillAtRandomOrReturnNull();
            if (chosenSkill == null)
            {
                EnemyActors[0].Move((PlayerActors[0].transform.position - EnemyActors[0].transform.position).normalized, () =>
                {
                    var chosenSkill = EnemyActors[0].ChooseValidSkillAtRandomOrReturnNull();
                    if (chosenSkill == null)
                        SwitchToNextTurn();
                    else
                        EnemyActors[0].StartCombo(SwitchToNextTurn);
                });
            }
            else
                EnemyActors[0].StartCombo(SwitchToNextTurn);
        }
        private void SwitchToNextTurn()
        {
            StartCoroutine(WaitForMovementCompletion(() =>
            {
                if(TestBattleOver())
                {
                    End();
                    return;
                }

                EnemyActors[0].ResetFlags();
                PlayerActors[0].ResetFlags();

                // Check if a repeat turn is requested
                if (repeatTurn)
                {
                    // If repeat turn, set isRepeatTurn to false for next turn
                    repeatTurn = false;

                    // Repeat the turn for the same actor
                    if (activeBattler == PlayerActors[0])
                        StartPlayerTurn();
                    else
                        StartEnemyTurn();
                }
                else
                {
                    // Proceed to the next turn normally
                    activeBattler = (activeBattler == PlayerActors[0]) ? EnemyActors[0] : PlayerActors[0];

                    if (activeBattler == PlayerActors[0])
                        StartPlayerTurn();
                    else
                        StartEnemyTurn();
                }
            }));
        }

        public IEnumerator WaitForMovementCompletion(Action onMovementComplete)
        {
            bool allActorsIdle = false;

            while (!allActorsIdle)
            {
                allActorsIdle = true;

                foreach (var actor in PlayerActors.Concat(EnemyActors))
                {
                    if (actor.currentState != MoveState.Idle)
                    {
                        allActorsIdle = false;
                        break; // At least one actor is still moving, exit the loop
                    }
                }
                yield return null; // Wait for the next frame
            }

            // All actors are now idle (no movement)
            // Proceed with further actions or logic
            onMovementComplete?.Invoke();

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