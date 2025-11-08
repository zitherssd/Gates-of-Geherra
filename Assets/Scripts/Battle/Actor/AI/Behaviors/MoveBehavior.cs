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
            if (actor.state.IsMoving(out MoveAction action))
            {
                action.Direction = direction;
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
            actor.state.IsMoving(out MoveAction moveAction);
            moveAction.Direction = direction;
            return NodeState.Sucess;
        }
    }

    public class CancelMove : BTNode
    {
        public override NodeState Execute(AIBT ai, Actor actor)
        {
            if (actor.state.IsMoving(out MoveAction moveAction))
            {
                moveAction.OnCancel?.Invoke();
                return NodeState.Sucess;
            }
            return NodeState.Failure;
        }
    }


    public class MoveAwayFromPlayer : BTNode
    {
        private float moveDuration;     // how long max we can move at once
        private float moveTimer = 0f;

        private float cooldown = 10f;   // cooldown after finishing
        private float cooldownTimer = 0f;

        private bool isOnCooldown = false;

        public MoveAwayFromPlayer(float moveDuration, float cooldown = 10f)
        {
            this.moveDuration = moveDuration;
            this.cooldown = cooldown;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            // 🔥 check cooldown first
            if (isOnCooldown)
            {
                cooldownTimer += Time.deltaTime;
                if (cooldownTimer >= cooldown)
                {
                    isOnCooldown = false;
                    cooldownTimer = 0f;
                }
                return NodeState.Failure; // not allowed to move away right now
            }

            MoveAction moveAction;
            if (actor.state.IsMoving(out moveAction))
            {
                moveAction.Direction = actor.target.DirectionToClosestEnemy * -1f;
                moveTimer += Time.deltaTime;

                if (moveTimer > moveDuration)
                {
                    moveAction.OnCancel?.Invoke();
                    moveTimer = 0f;
                    isOnCooldown = true; // 🔥 start cooldown
                    return NodeState.Sucess;
                }

                return NodeState.Running;
            }
            else
            {
                moveTimer = 0f; // reset if not moving
            }

            // start moving away
            var moveSkill = actor.ActorData.actions.OfType<MoveAction>().FirstOrDefault();
            if (moveSkill == null) return NodeState.Failure;

            moveSkill.Direction = actor.target.DirectionToClosestEnemy * -1f;
            actor.UseAction(moveSkill, actor.state.TransitionToIdle);
            return NodeState.Running;
        }
    }
}
