using Assets.Scripts.Pattern;
using System;
using UnityEngine;
using static Assets.BaseAction;

namespace Assets.Scripts.Battle.Components.State
{
    public class IdleState : IState //Ready state
    {
        private Battle.Actor actor;
        private SpriteRenderer spriteRenderer;
        private float rampupfactor;
        public event Action OnEnterIdle;


        public IdleState(Battle.Actor actor)
        {
            this.actor = actor;
            spriteRenderer = actor.GetComponent<SpriteRenderer>();
        }

        public void Enter()
        {
            //deathcheck this needs to be moved
            DeathCheck();
            if (actor.isControllable)
            {
                //  var AvaliableSkills = actor.ActorData.actions;  //This should be read at the beginning of the fight?
                UIManager.GetInstance().ShowUI();
                var Ready = true;
            }

            //actor.state.TransitionTo(actor.state.moveState.SetForTarget(10f, null)); do this from ai
            //if controllable then you just need to activate UI
            //if not controllable the ai takes over from another script no need to do anything
            if (OnEnterIdle != null)
                OnEnterIdle.Invoke();
        }

        private void DeathCheck()
        {
            if (actor.ActorData.GetCurrentHP() == 0)
            {
                actor.state.TransitionTo(actor.state.deathState);
                BattleManager.instance.End();
                return;
            }
        }

        public void Exit()
        {

        }

        public void OnCollisionEnter(Collision collision)
        {
        }

        public void Update()
        {
        }
    }
}