using System.Linq;
using Assets.Scripts.Battle.Actions.Actions;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class DashBehavior : BTNode
    {
        public override NodeState Execute(AIBT ai, Actor actor)
        {
            var chargeSkill = actor.Runtime.actions.OfType<Charge>()
                .FirstOrDefault(action => action.IsValid(actor, out _));



            if (chargeSkill == null || actor.Rb.velocity.magnitude > 0.5f || actor.target.DistanceToClosestEnemy < 2.5f)
                return NodeState.Failure;

            actor.UseAction(chargeSkill);
                return NodeState.Sucess;
        }
    }
}
