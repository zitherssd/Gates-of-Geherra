using Assets.Scripts.Battle.Actor.AI.Behaviors;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Battle.Actor.AI.Conditions
{
    public class HesitateCondition : BTNode
    {
        private float requiredWaitTime;
        private float currentWaitTimer;
        private float detectionRange;

        public HesitateCondition(float detectionRange, float requiredWaitTime)
        {
            this.detectionRange = detectionRange;
            this.requiredWaitTime = requiredWaitTime;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            if (actor.target != null && actor.target.DistanceToClosestEnemy <= detectionRange)
            {
                currentWaitTimer += Time.deltaTime;
                if (currentWaitTimer >= requiredWaitTime)
                {
                    return NodeState.Sucess;
                }
            }
            else
            {
                currentWaitTimer = 0f;
            }

            return NodeState.Failure;
        }
    }
}