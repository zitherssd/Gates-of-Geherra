using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Actions;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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
            actor.UseAction(moveSkill);
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
                actor.movement.agent.SetDestination(actor.target.ClosestEnemy.transform.position);
                moveAction.Direction = actor.movement.agent.desiredVelocity.normalized;
                timer += Time.deltaTime;
                if (timer > duration)
                {
                    moveAction.EndAction(ActionEndReason.Cancelled);
                    return NodeState.Sucess;
                }
                return NodeState.Running;
            }
            else
                timer = 0f;

            var moveSkill = actor.ActorData.actions.OfType<MoveAction>().FirstOrDefault();
            if (moveSkill == null) return NodeState.Failure;

            moveSkill.Direction = actor.movement.agent.desiredVelocity.normalized;
            actor.UseAction(moveSkill);
            return NodeState.Running;
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
                    moveAction.EndAction(ActionEndReason.Cancelled);
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
            actor.UseAction(moveSkill);
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
                moveAction.EndAction(ActionEndReason.Cancelled);
                return NodeState.Sucess;
            }
            return NodeState.Failure;
        }
    }



    public class MoveAwayFromLevel : BTNode
    {
        private float moveDuration;
        private float moveTimer = 0f;
        private float rayLength;

        private readonly Vector3[] directions = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized
        };

        public MoveAwayFromLevel(float moveDuration = 1f, float rayLength = 3f)
        {
            this.moveDuration = moveDuration;
            this.rayLength = rayLength;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            MoveAction moveAction;
            if (actor.state.IsMoving(out moveAction))
            {
                moveTimer += Time.deltaTime;
                if (moveTimer > moveDuration)
                {
                    moveAction.EndAction(ActionEndReason.Completed);
                    moveTimer = 0f;
                    return NodeState.Sucess;
                }
                return NodeState.Running;
            }

            // Find the nearest "Level" obstacle
            Transform transform = actor.transform;
            Vector3 bestDir = Vector3.zero;
            float closestDist = float.MaxValue;

            foreach (var dir in directions)
            {
                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, rayLength))
                {
                    if (hit.collider.CompareTag("Level") && hit.distance < closestDist)
                    {
                        closestDist = hit.distance;
                        bestDir = dir;
                    }
                }
            }

            // Move away from the nearest detected obstacle
            if (bestDir != Vector3.zero)
            {
                var moveSkill = actor.ActorData.actions.OfType<MoveAction>().FirstOrDefault();
                if (moveSkill == null) return NodeState.Failure;

                moveSkill.Direction = -bestDir;
                actor.UseAction(moveSkill);
                return NodeState.Running;
            }

            // No nearby obstacle
            return NodeState.Sucess;
        }
    }

    public class ApproachPlayerIntelligently : BTNode
    {
        private float flankDistance = 2.5f;
        private float closeRange = 1.5f;

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            var agent = actor.GetComponent<NavMeshAgent>();
            var moveAction = actor.ActorData.actions.OfType<MoveAction>().First();

            Vector3 targetPos = actor.target.ClosestEnemy.transform.position;
            float dist = Vector3.Distance(actor.transform.position, targetPos);

            // Too close? Try flanking instead of walking straight.
            if (dist < closeRange)
            {
                Vector3 flank = GetFlankPosition(actor);
                agent.SetDestination(flank);
            }
            else
            {
                agent.SetDestination(targetPos);
            }

            moveAction.Direction = agent.desiredVelocity.normalized;

            actor.UseAction(moveAction);
            return NodeState.Running;
        }

        private Vector3 GetFlankPosition(Actor actor)
        {
            Vector3 right = actor.transform.right;
            Vector3 left = -right;

            // random flank (left or right)
            Vector3 flankDir = (UnityEngine.Random.value > 0.5f) ? right : left;

            return actor.target.ClosestEnemy.transform.position + flankDir * 2.5f;
        }
    }
}
