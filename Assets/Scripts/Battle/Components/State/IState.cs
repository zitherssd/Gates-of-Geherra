using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State
{
    public interface IState
    {
        public void Enter();
        public void Update();
        public void Exit();
        public void OnCollisionEnter(Collision collision);
    }
}