using System.Collections;
using System.Collections.Generic;
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

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            SetupBattle();
            SetActiveCharacterBattle(PlayerActors[0]);
            state = State.WaitingForPlayer;
        }

        void SetupBattle()
        {
            //Assing the enemies. This is mostly done in the inspctor.
            //Units are already spawned.
            //chose dialogue
            foreach (var actor in PlayerActors)
            {
                actor.GetBaseActor().Initialize();
                actor.GetBaseActor().Controllable = true;
            }
            foreach (var actor in EnemyActors)
            {
                actor.GetBaseActor().Initialize();
                actor.GetBaseActor().Controllable = false;
            }
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

        private bool TestBattleOver()
        {
            //foreach battler in PlayerActors check if dead // show enemy wins return true
            //foreach battler in enemyactors check if dead // show player wins return true
            if (PlayerActors.TrueForAll(actor => actor.GetBaseActor().GetCurrentHP() == 0))
            {
                UIManager.GetInstance().ChangeStatus("YOU LOSE");
                return true;
            }
            if (EnemyActors.TrueForAll(actor => actor.GetBaseActor().GetCurrentHP() == 0))
            {
                UIManager.GetInstance().ChangeStatus("YOU WIN!!");
                return true;
            }
            return false;
        }

        private void ChooseNextActiveCharacter()
        {
            if (TestBattleOver()) { return; }

            EnemyActors[0].isBlocking = false;
            PlayerActors[0].isBlocking = false;

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

                var chosenSkill = EnemyActors[0].ChooseValidSkillAtRandomOrReturnNull();
                if (chosenSkill != null)
                    EnemyActors[0].UseSkill(chosenSkill, ChooseNextActiveCharacter);
                else
                    //Move closer
                    EnemyActors[0].Move((PlayerActors[0].transform.position - EnemyActors[0].transform.position).normalized, ChooseNextActiveCharacter);
            }
            else
            {
                UIManager.GetInstance().ChangeStatus("PLAYER TURN");
                SetActiveCharacterBattle(PlayerActors[0]);
                state = State.WaitingForPlayer;
            }
        }

        private void HandleRepeatTurn()
        {
            SetActiveCharacterBattle(activeBattler);

            if (activeBattler == PlayerActors[0])
            {
                state = State.WaitingForPlayer;
            }
            else
            {
                EnemyActors[0].UseSkill(EnemyActors[0].ChooseValidSkillAtRandom(), () => { ChooseNextActiveCharacter(); });
            }
        }

        public void RepeatTurn()
        {
            repeatTurn = true;
        }


        private void Update()
        {
            if (state == State.WaitingForPlayer)
            {
                InputManager.GetInstance().WaitForTurn(() =>
                {
                    ChooseNextActiveCharacter(); //End the turn on skill completion;
                });
                state = State.Busy;
            }
        }

        public BaseActorBattler GetActiveActor()
        {
            return activeBattler;
        }

        public void EndActiveActorTurn()
        {

        }

        IEnumerator wait(float seconds)
        {
            float counter = 0;
            while (counter < seconds)
            {
                counter += Time.deltaTime;
                yield return null;
            }
        }
    }
}