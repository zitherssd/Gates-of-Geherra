using Assets.Scripts.Battle.Actor.AI.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Battle.Actor.AI.Conditions
{
    public class StaminaLesserThan : BTNode
    {
        private float percentOfMaximum;
        public StaminaLesserThan(float percentOfMaximum)
        {
            this.percentOfMaximum = percentOfMaximum;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            if (actor.Runtime.currentStamina < actor.Runtime.maxStamina * percentOfMaximum)
            {
                return NodeState.Sucess;
            }
            return NodeState.Failure;
        }
    }

    public class StaminaGreaterThan : BTNode
    {
        private float percentOfMaximum;
        public StaminaGreaterThan(float percentOfMaximum)
        {
            this.percentOfMaximum = percentOfMaximum;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            if (actor.Runtime.currentStamina > actor.Runtime.maxStamina * percentOfMaximum)
            {
                return NodeState.Sucess;
            }
            return NodeState.Failure;
        }
    }
}
