using System;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Pattern;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
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

            // Ignore collision with all enemy actors
            foreach (var enemyActor in BattleManager.instance.EnemyActors)
            {
                var enemyCollider = enemyActor.GetComponent<CapsuleCollider>();
                if (enemyCollider != null)
                {
                    Physics.IgnoreCollision(cc, enemyCollider);
                }
            }

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