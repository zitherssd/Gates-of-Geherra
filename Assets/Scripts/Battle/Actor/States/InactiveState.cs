using System;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class InactiveState : IState //Ready state
    {
        private Actor actor;
        private SpriteRenderer spriteRenderer;
        private float rampupfactor;
        public event Action OnEnterIdle;


        public InactiveState(Actor actor)
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
        }

        public void Update()
        {
        }
    }
}