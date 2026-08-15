using Assets.Scripts.Battle.Actions.Skills;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    /// <summary>
    /// Sidesteps the actor laterally (perpendicular to the line to the player)
    /// until it finds an angle where its throw will not clip friendly enemies.
    ///
    /// Returns Running while repositioning, Success once the line of fire is
    /// clear, and Failure if the actor has no MoveAction or cannot path the
    /// chosen direction (letting lower-priority behaviors take over). Stateless,
    /// so it is safe to share across actors via the static behavior lists.
    /// </summary>
    public class RepositionForClearShot : BTNode
    {
        private const float ClearRadius = 0.5f;   // corridor half-width that counts as blocked
        private const float Height = 0.5f;        // height the corridor is checked at
        private const float StepDistance = 2.5f;  // how far to sidestep per reposition
        private const float CutInDistance = 2f;   // how far to also step in toward the player
        private const float MinRange = 3f;      // keep above the 3u "too close" threshold

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            var moveSkill = actor.Runtime.actions.OfType<MoveAction>().FirstOrDefault();
            if (moveSkill == null) return NodeState.Failure;

            var agent = actor.movement.agent;
            if (agent == null || actor.target.ClosestEnemy == null) return NodeState.Failure;

            // Already in a spot with a clear shot.
            if (!FriendlyFireCheck.IsBlocked(actor, ClearRadius, Height))
                return NodeState.Sucess;

            // Cut in toward the player while sidestepping: closing the distance widens
            // the shooting angle, so the blocking friendly clears the line of fire much
            // sooner than a pure lateral step. The cut-in is capped so the thrower never
            // dips below MinRange (avoids flip-flopping with the MoveAwayFromPlayer node).
            Vector3 toPlayer = actor.target.ClosestEnemy.transform.position - actor.transform.position;
            toPlayer.y = 0f;
            toPlayer.Normalize();
            float maxCutIn = Mathf.Max(0f, actor.target.DistanceToClosestEnemy - MinRange);
            float cutIn = Mathf.Min(CutInDistance, maxCutIn);

            // Keep sidestepping away from the nearest blocker until the shot clears.
            Vector3 lateral = FriendlyFireCheck.ChooseSidestepDirection(actor, Height);
            Vector3 destination = actor.transform.position + lateral * StepDistance + toPlayer * cutIn;
            if (!agent.SetDestination(destination))
                return NodeState.Failure; // can't move that way; let other behaviors take over

            moveSkill.Direction = agent.desiredVelocity.normalized;
            actor.UseAction(moveSkill);
            return NodeState.Running;
        }
    }
}
