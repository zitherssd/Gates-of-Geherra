using Assets.Scripts.Pattern;
using Assets.Scripts.Utility;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Battle.Manager.States
{
    public class BattleActiveState : IState
    {
        public event Action FinalHitDealth;
        private BattleManager battleManager;
        private bool exitStep = false;
        public BattleActiveState(BattleManager manager)
        {
            battleManager = manager;
        }
        public void Enter()
        {
            exitStep = false;
            //Do initialization of all actors


            // battleManager.battleStateMachine.TransitionTo(battleManager.battleStateMachine.startState);
        }

        public void Exit()
        {
        }

        public void OnCollisionEnter(Collision collision)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            if (exitStep == false)
            {
                if (battleManager.EnemyActors.TrueForAll(actor => actor.ActorData.isDead())) // Last hit dealt now
                {
                    FinalHitDealth?.Invoke();
                    //Gain meter, final hit effects
                    UIManager.instance.ResetMeter();
                    UIManager.instance.GainMeter(2f);
                    UIManager.instance.DisableUI();
                    CameraManager.instance.SlowTrack = true;
                    exitStep = true;
                }

                if (battleManager.PlayerActors.TrueForAll(actor => actor.ActorData.isDead())) // Last hit dealt now
                {
                    //Gain meter, final hit effects
                    UIManager.instance.ResetMeter();
                    UIManager.instance.GainMeter(2f);
                    UIManager.instance.DisableUI();
                    CameraManager.instance.SlowTrack = true;
                    exitStep = true;
                }
            }
            else
            {
                if (battleManager.EnemyActors.TrueForAll(actor => !actor.state.IsAlive())) // All enemies are in death state
                {
                    battleManager.battleStateMachine.TransitionTo(battleManager.battleStateMachine.endState); //Transition to end state
                }
                else if (battleManager.PlayerActors.TrueForAll(actor => !actor.state.IsAlive())) // All players are in death state
                {
                    battleManager.StartCoroutine(battleManager.WaitForSeconds(2f, () => { SceneManager.LoadScene("TitleScene"); }));
                }

            }
        }
    }
}
