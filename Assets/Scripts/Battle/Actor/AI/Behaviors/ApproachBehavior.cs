using System.Linq;
using Assets.Scripts.Battle.Actions.Actions;
using Assets.Scripts.Pattern;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class ApproachBehavior : BTNode
    {
        public override NodeState Execute(AIBT ai, Actor actor)
        {
            MoveAction moveaction;
            if (actor.state.IsMoving(out moveaction))
            {
                actor.movement.agent.SetDestination(actor.target.ClosestEnemy.transform.position);
                moveaction.Direction = actor.movement.agent.desiredVelocity.normalized;
                if (actor.target.DistanceToClosestEnemy < 1.4f) moveaction.OnCancel?.Invoke();
            }

            if (actor.Rb.velocity.magnitude > 0.5f || actor.target.DistanceToClosestEnemy < 1.1f)
                return NodeState.Failure;


            var moveSkill = actor.ActorData.actions.OfType<MoveAction>().FirstOrDefault();
            if (moveSkill == null) return NodeState.Failure;

            moveSkill.Direction = actor.target.DirectionToClosestEnemy;
            actor.UseAction(moveSkill, actor.state.TransitionToIdle);
            return NodeState.Sucess;
        }
    }
}
