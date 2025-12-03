using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public class GameFlowManager : MonoBehaviour
    {
        public static GameFlowManager instance;
        public Actor playerActor;
        public RestAreaManager restAreaManager;
        public BattleManager battleManager;
        public int trainingsDone = 0;
        public int trainingsDoneThisFloor = 0;
        public List<TimeLock> timelocks;


        //Player should be spawned and loaded or created here all other managers assume player exists


        public void Start()
        {
            if (SaveManager.instance == null) {
                ActorData template = Resources.Load<ActorData>("Actors/MC");
                ActorData clone = Instantiate(template);
                playerActor.ActorData = clone;
                RollNewstats(playerActor);
                playerActor.Spawn();

                SetMode(GameMode.RestArea); return; 
            }

            if (SaveManager.instance.SlotExists(SaveManager.instance.currentSaveSlot))
            {
                var save = SaveManager.instance.LoadFromSlot(SaveManager.instance.currentSaveSlot);
                save.LoadActor(playerActor);
            }
            else
            {
                ActorData template = Resources.Load<ActorData>("Actors/MC");
                ActorData clone = Instantiate(template);
                playerActor.ActorData = clone;
                playerActor.ActorData.Name = SaveManager.instance.newGamePlayerName;
                RollNewstats(playerActor);
                playerActor.Spawn();
                SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
            }

            //Now that player is loaded we go to rest
            SetMode(GameMode.RestArea);
            UIManager.instance.InitializePlayerActionButtonPrefabs(playerActor.ActorData.actions);
            UIManager.instance.DisableBattleSkills();
        }

        public GameMode Mode { get; private set; }

        public void Awake()
        {
            instance = this;
            if (timelocks == null)
                timelocks = new List<TimeLock>();
        }


        public void EnterBattle(BattleDefinition battle, Action onBattleEnd )
        {

            Mode = GameMode.Battle;
            restAreaManager.enabled = false;
            TrainingManager.instance.enabled = false;
            battleManager.enabled = true;
            battleManager.Enter(battle, onBattleEnd);
        }

        public void SetMode(GameMode mode)
        {
            Mode = mode;

            restAreaManager.enabled = false;
            //explorationManager.enabled = false;
            //trainingManager.enabled = false;
            battleManager.enabled = false;

            switch (Mode)
            {
                case GameMode.RestArea:
                    restAreaManager.enabled = true;
                    TrainingManager.instance.enabled = true;
                    restAreaManager.Enter();
                    break;

                case GameMode.Training:

                    //trainingManager.Enter();
                    break;

                case GameMode.Exploring:
                    //explorationManager.Enter();
                    break;

                case GameMode.Battle:
                    //battleManager.BeginBattle(currentBattle);
                    break;
            }
        }

        public void RollNewstats(Actor playerActor)
        {
            var RandomStr = UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7);
            var RandomAgi = UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7);
            var RandomMnd = UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7);
            var RandomSpi = UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7);

            playerActor.ActorData.Strength = RandomStr;
            playerActor.ActorData.Agility = RandomAgi;
            playerActor.ActorData.Mind = RandomMnd;
            playerActor.ActorData.Spirit = RandomSpi;
        }
    }
    public enum GameMode
    {
        RestArea,
        Training,
        Exploring,
        Battle,
        StoryBattle,
        Timeout,
    }


}


