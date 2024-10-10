using Assets.Scripts.Pattern;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State.States
{
    public class DeathState : IState
    {
        private readonly Actor owner;

        public DeathState(Actor owner)
        {
            this.owner = owner;
        }

        public void Enter()
        {
            owner.PlayAnimation("Down");
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