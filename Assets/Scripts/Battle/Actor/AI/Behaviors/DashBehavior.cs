using Assets.Scripts.Battle.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Battle.Components.AI.Behaviors
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
