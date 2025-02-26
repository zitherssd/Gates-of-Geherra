using System.Linq;
using Assets.Scripts.Battle.Actions.Actions;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class ApproachBehavior : IAIBehavior
    {
        public bool Execute(AISystem ai, Actor actor)
        {

            if (actor.Rb.velocity.magnitude > 0.5f || actor.target.DistanceToClosestEnemy < 1.4f)
                return false;


            var moveSkill = actor.ActorData.actions.OfType<MoveAction>().FirstOrDefault();
            if (moveSkill == null) return false;

            moveSkill.Direction = actor.target.DirectionToClosestEnemy;
            actor.UseAction(moveSkill, actor.state.TransitionToIdle);
            return true;
        }
    }
}
