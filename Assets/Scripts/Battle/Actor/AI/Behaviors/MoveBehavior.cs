using Assets.Scripts.Battle.Actions.Actions;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class MoveBehavior : BTNode
    {
        UnityEngine.Vector3 direction;
        public MoveBehavior(UnityEngine.Vector3 direction, float duration)
        {
            this.direction = direction;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            if (actor.state.CurrentState == actor.state.moveState)
            {
                actor.state.moveState.action.Direction = direction;
            }

            var moveSkill = actor.ActorData.actions.OfType<MoveAction>().FirstOrDefault();
            if (moveSkill == null) return NodeState.Failure;

            moveSkill.Direction = direction;
            actor.UseAction(moveSkill, actor.state.TransitionToIdle);
            return NodeState.Running;
        }
    }

    public class MoveTowardsPlayer : BTNode
    {
        float duration;
        float timer = 0f;
        public MoveTowardsPlayer(float duration)
        {
            this.duration = duration;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            MoveAction moveAction;
            if (actor.state.IsMoving(out moveAction))
            {
                moveAction.Direction = actor.target.DirectionToClosestEnemy;
                timer += Time.deltaTime;
                if (timer > duration)
                {
                    moveAction.OnCancel?.Invoke();
                    return NodeState.Sucess;
                }
                return NodeState.Running;
            }
            else
                timer = 0f;

            var moveSkill = actor.ActorData.actions.OfType<MoveAction>().FirstOrDefault();
            if (moveSkill == null) return NodeState.Failure;

            moveSkill.Direction = actor.target.DirectionToClosestEnemy;
            actor.UseAction(moveSkill, actor.state.TransitionToIdle);
            return NodeState.Running;
        }
    }

    public class SetMoveDirection : BTNode
    {

        Vector3 direction;
        public SetMoveDirection(Vector3 direction)
        {
            this.direction = direction;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            actor.state.moveState.action.Direction = direction;
            return NodeState.Sucess;
        }
    }

    public class CancelMove : BTNode
    {
        public override NodeState Execute(AIBT ai, Actor actor)
        {
            if (actor.state.IsMoving(out _))
            {
                actor.state.moveState.action.OnCancel?.Invoke();
                return NodeState.Sucess;
            }
            return NodeState.Failure;
        }
    }


    public class MoveAwayFromPlayer : BTNode
    {
        float duration;
        float timer = 0f;
        public MoveAwayFromPlayer(float duration)
        {
            this.duration = duration;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            MoveAction moveAction;
            if (actor.state.IsMoving(out moveAction))
            {
                moveAction.Direction = actor.target.DirectionToClosestEnemy * -1f;
                timer += Time.deltaTime;
                if (timer > duration)
                {
                    moveAction.OnCancel?.Invoke();
                    return NodeState.Sucess;
                }
                return NodeState.Running;
            }
            else
                timer = 0f;

            var moveSkill = actor.ActorData.actions.OfType<MoveAction>().FirstOrDefault();
            if (moveSkill == null) return NodeState.Failure;

            moveSkill.Direction = actor.target.DirectionToClosestEnemy * -1f;
            actor.UseAction(moveSkill, actor.state.TransitionToIdle);
            return NodeState.Running;
        }
    }
}
