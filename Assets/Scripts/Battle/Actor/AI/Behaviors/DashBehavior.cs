using System.Linq;
using Assets.Scripts.Battle.Actions.Actions;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class DashBehavior : IAIBehavior
    {
        public bool Execute(AISystem ai, Actor actor)
        {
            var chargeSkill = actor.ActorData.actions.OfType<Charge>()
                .FirstOrDefault(action => action.IsValid(actor, out _));



            if (chargeSkill == null || actor.Rb.velocity.magnitude > 0.5f || actor.target.DistanceToClosestEnemy < 2.5f)
                return false;

            actor.UseAction(chargeSkill, actor.state.TransitionToIdle);
                return true;
        }
    }
}
