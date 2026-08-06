using System.Linq;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class ApproachBehavior : BTNode
    {
        private const float StopDistance = 1.2f;   // Close enough to stop and let attack behaviors run.
        private const float RayLength = 0.5f;       // Front / left / right probe distance.
        private const float SideStepDistance = 1.5f; // How far to sidestep around a blocking actor.
        private const float RayHeight = 0.5f;       // Elevate ray origin above the actor's feet.

        // Reused buffer so the raycasts don't allocate every AI tick.
        private readonly RaycastHit[] hitBuffer = new RaycastHit[8];

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            MoveAction moveaction;
            bool isMoving = actor.state.IsMoving(out moveaction);

            if (actor.target.DistanceToClosestEnemy < StopDistance)
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

            // We are far. Figure out where to go this tick.
            Vector3 destination = PickDestination(actor);

            if (isMoving)
            {
                // We are already moving, update direction and let it run.
                actor.movement.agent.SetDestination(destination);
                moveaction.Direction = actor.movement.agent.desiredVelocity.normalized;
                return NodeState.Sucess;
            }
            else
            {
                // We are not moving, so start moving.
                var moveSkill = actor.Runtime.actions.OfType<MoveAction>().FirstOrDefault();
                if (moveSkill == null) return NodeState.Failure;

                actor.movement.agent.SetDestination(destination);
                moveSkill.Direction = actor.movement.agent.desiredVelocity.normalized;
                actor.UseAction(moveSkill);
                return NodeState.Sucess;
            }
        }

        /// <summary>
        /// Pick a NavMesh destination for this tick: go straight at the player, but
        /// if another ACTOR is blocking the lane directly ahead, sidestep into a
        /// clear left or right lane. Only other actors count as blockers — level
        /// geometry is left to the NavMeshAgent. If every lane is blocked, keep
        /// drifting to the actor's preferred side until a lane opens up.
        /// </summary>
        private Vector3 PickDestination(Actor actor)
        {
            Vector3 forward = actor.target.DirectionToClosestEnemy;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                forward = actor.transform.forward;
            forward.Normalize();

            // Front lane clear -> head straight for the player.
            if (!IsBlockedByActor(actor, forward))
                return actor.target.ClosestEnemy.transform.position;

            // Front is blocked by another actor. Probe the side lanes.
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 left = -right;

            bool leftClear = !IsBlockedByActor(actor, left);
            bool rightClear = !IsBlockedByActor(actor, right);

            int side; // -1 = left, +1 = right
            if (leftClear && !rightClear)
                side = -1;
            else if (rightClear && !leftClear)
                side = 1;
            else
            {
                // Both lanes open, OR every lane blocked — keep drifting to a
                // deterministic side from the actor's instance ID so it never
                // flip-flops between frames. (BT nodes are shared per ruleset, so
                // we can't cache per-actor state here.) When everything is blocked
                // this keeps the actor pushing sideways until a lane opens.
                side = (Mathf.Abs(actor.GetInstanceID()) % 2 == 0) ? -1 : 1;
            }

            // Sidestep: aim at a point beside our current position. Re-evaluated
            // every tick, so we keep drifting sideways until the front lane clears.
            Vector3 lateral = side < 0 ? left : right;
            return actor.transform.position + lateral * SideStepDistance;
        }

        /// <summary>
        /// True if any collider belonging to a DIFFERENT actor lies within RayLength
        /// along <paramref name="direction"/> from the actor. Self-hits and anything
        /// without an Actor component (level geometry, props) are ignored.
        /// </summary>
        private bool IsBlockedByActor(Actor actor, Vector3 direction)
        {
            Vector3 origin = actor.transform.position + Vector3.up * RayHeight;
            int count = Physics.RaycastNonAlloc(origin, direction, hitBuffer, RayLength);
            for (int i = 0; i < count; i++)
            {
                Actor other = hitBuffer[i].collider.GetComponentInParent<Actor>();
                if (other != null && other != actor)
                    return true;
            }
            return false;
        }
    }
}
