using Assets.Scripts.Battle.Actor.AI.Behaviors;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Battle.Actor.AI.Conditions
{
    public class GlobalAttackTokenCondition : BTNode
    {
        private static float lastGlobalAttackTime;
        private float globalCooldown;

        public GlobalAttackTokenCondition(float cooldownBetweenEnemyAttacks)
        {
            this.globalCooldown = cooldownBetweenEnemyAttacks;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            if (Time.time - lastGlobalAttackTime >= globalCooldown)
            {
                lastGlobalAttackTime = Time.time;
                return NodeState.Sucess;
            }
            return NodeState.Failure;
        }
    }
}