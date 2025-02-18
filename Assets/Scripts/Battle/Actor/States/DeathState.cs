using Assets.Scripts.Pattern;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State.States
{
    public class DeathState : IState
    {
        private readonly Actor owner;
        private CapsuleCollider cc;
        public Action OnDeath;

        public DeathState(Actor owner)
        {
            this.owner = owner;
            cc = owner.GetComponent<CapsuleCollider>();
        }

        public void Enter()
        {
            if (owner.isControllable)
            {
                UIManager.GetInstance().HideUI();
            }
            OnDeath?.Invoke();
            owner.PlayAnimation("Down");
            Physics.IgnoreCollision(cc, BattleManager.instance.PlayerActors[0].GetComponent<CapsuleCollider>());
        }

        public void Exit()
        {
            cc.enabled = true;
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