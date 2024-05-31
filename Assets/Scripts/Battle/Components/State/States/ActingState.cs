using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State.States
{
    public class ActingState : IState
    {
        private readonly Actor owner;
        public ActingState(Actor owner)
        {
            this.owner = owner;
        }
        public void Enter()
        {
        }

        public void Exit()
        {
        }

        public void Update()
        {
        }
    }
}