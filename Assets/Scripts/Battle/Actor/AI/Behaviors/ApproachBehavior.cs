using System.Linq;
using Assets.Scripts.Battle.Actions.Actions;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class ApproachBehavior : BTNode
    {
        public override NodeState Execute(AIBT ai, Actor actor)
        {

            if (actor.state.CurrentState == actor.state.moveState)
            {
                actor.state.moveState.action.Direction = actor.target.DirectionToClosestEnemy;
                if (actor.target.DistanceToClosestEnemy < 1.4f) actor.state.moveState.action.OnCancel?.Invoke();
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
