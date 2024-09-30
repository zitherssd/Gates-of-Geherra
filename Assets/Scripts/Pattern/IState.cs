using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Pattern
{
    public interface IState
    {
        public void Enter();
        public void Update();
        public void Exit();
        public void OnCollisionEnter(Collision collision);
    }
}