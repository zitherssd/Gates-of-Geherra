using Assets.Scripts.Pattern;
using System.Collections;
using UnityEngine;
using Assets.Scripts.Pattern;


namespace Assets.Scripts.Battle.Components.State
{
    public class IdleState : IState
    {
        private Battle.Actor actor;
        private SpriteRenderer spriteRenderer;

        public IdleState(Battle.Actor actor)
        {
            this.actor = actor;
            spriteRenderer = actor.GetComponent<SpriteRenderer>();
        }

        public void Enter()
        {

        }

        public void Exit()
        {

        }

        public void OnCollisionEnter(Collision collision)
        {
            //throw new System.NotImplementedException();
        }

        public void Update()
        {
            //
        }
    }
}