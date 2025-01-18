using Assets.Scripts.Battle.Actions;
using System;
using System.Linq;

namespace Assets.Scripts.Battle.Components.AI.Behaviors
{
    public class ApproachBehavior : IAIBehavior
    {
        public bool Execute(AIComponent ai, Actor actor)
        {



            if (actor.rigidbody.velocity.magnitude > 0.5f || actor.target.DistanceToClosestEnemy < 1.4f)
                return false;


            var moveSkill = actor.ActorData.actions.OfType<MoveAction>().FirstOrDefault();
            if (moveSkill == null) return false;

            moveSkill.Direction = actor.target.DirectionToClosestEnemy;
            actor.UseAction(moveSkill, actor.state.TransitionToIdle);
            return true;
        }
    }
}
