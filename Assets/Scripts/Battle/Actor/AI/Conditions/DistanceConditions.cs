using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Reactions;
using Assets.Scripts.Battle.Actions.Skills;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors

{
    public class DistanceToPlayerSmallerThan : BTNode
    {
        private float maximumDistance;
        public DistanceToPlayerSmallerThan(float maximumDistance)
        {
            this.maximumDistance = maximumDistance;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            if (actor.target.DistanceToClosestEnemy < maximumDistance)
            {
                    return NodeState.Sucess;
            }
            return NodeState.Failure;
        }
    }

    public class DistanceToPlayerGreaterThan : BTNode
    {
        private float minimumDistance;
        public DistanceToPlayerGreaterThan(float minimumDistance)
        {
            this.minimumDistance = minimumDistance;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {

            if (actor.target.DistanceToClosestEnemy > minimumDistance)
            {
                return NodeState.Sucess;
            }
            return NodeState.Failure;
        }
    }
}
