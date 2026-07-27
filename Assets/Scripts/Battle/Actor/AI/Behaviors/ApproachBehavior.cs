﻿using System.Linq;
using Assets.Scripts.Battle.Actions.Actions;
using Assets.Scripts.Pattern;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class ApproachBehavior : BTNode
    {
        public override NodeState Execute(AIBT ai, Actor actor)
        {
            MoveAction moveaction;
            bool isMoving = actor.state.IsMoving(out moveaction);

            if (actor.target.DistanceToClosestEnemy < 1.4f)
            {
                if (isMoving)
                {
                    // We are close and moving, so stop.
                    // Ending the action will cause a transition to Idle.
                    var currentAction = actor.GetCurrentAction();
                    if(currentAction != null && currentAction == moveaction)
                    {
                        currentAction.EndAction(Actions.ActionEndReason.Completed);
                    }
                }
                // Whether we were moving or not, we are close, so this behavior fails
                // to let other behaviors (like attack) run.
                return NodeState.Failure;
            }

            // We are far.
            if (isMoving)
            {
                // We are already moving, update direction and let it run.
                actor.movement.agent.SetDestination(actor.target.ClosestEnemy.transform.position);
                moveaction.Direction = actor.movement.agent.desiredVelocity.normalized;
                return NodeState.Sucess;
            }
            else
            {
                // We are not moving, so start moving.
                var moveSkill = actor.Runtime.actions.OfType<MoveAction>().FirstOrDefault();
                if (moveSkill == null) return NodeState.Failure;

                actor.movement.agent.SetDestination(actor.target.ClosestEnemy.transform.position);
                moveSkill.Direction = actor.movement.agent.desiredVelocity.normalized;
                actor.UseAction(moveSkill);
                return NodeState.Sucess;
            }
        }
    }
}
