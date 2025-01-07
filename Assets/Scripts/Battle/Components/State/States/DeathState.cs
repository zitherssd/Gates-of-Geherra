using Assets.Scripts.Pattern;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State.States
{
    public class DeathState : IState
    {
        private readonly Actor owner;
        private CapsuleCollider cc;

        public DeathState(Actor owner)
        {
            this.owner = owner;
            cc = owner.GetComponent<CapsuleCollider>();
        }

        public void Enter()
        {
            owner.PlayAnimation("Down");
            cc.height = 0.1f;
            cc.center = new Vector3(0, 0.05f, 0);
            cc.radius = 0.01f;
        }

        public void Exit()
        {
            cc.height = 1.2f;
            cc.center = new Vector3(0, 0.6f, 0);
            cc.radius = 0.25f;
        }

        public void OnCollisionEnter(Collision collision)
        {
        }

        public void Update()
        {
        }
    }
}