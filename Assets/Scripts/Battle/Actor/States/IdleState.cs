using System;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Pattern;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class IdleState : IState //Ready state
    {
        private Actor actor;
        private SpriteRenderer spriteRenderer;
        public event Action OnEnterIdle;


        public IdleState(Actor actor)
        {
            this.actor = actor;
            spriteRenderer = actor.GetComponent<SpriteRenderer>();
        }

        public void Enter()
        {
            //deathcheck this needs to be moved
            if (actor.isControllable)
            {
                //  var AvaliableSkills = actor.Runtime.actions;  //This should be read at the beginning of the fight?
                UIManager.GetInstance().ShowUI();
                var Ready = true;
                
                // Activate state-based slowdown when entering idle
                if (SlowdownManager.instance != null)
                    SlowdownManager.instance.EnterStateSlowdown();
            }

            //actor.state.TransitionTo(actor.state.moveState.SetForTarget(10f, null)); do this from ai
            //if controllable then you just need to activate UI
            //if not controllable the ai takes over from another script no need to do anything
            if (OnEnterIdle != null)
                OnEnterIdle.Invoke();
        }

        private void DeathCheck()
        {
            if (actor.Runtime.isDead())
            {
                actor.state.TransitionTo<DeathState>();
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
            DeathCheck();
        }
    }
}