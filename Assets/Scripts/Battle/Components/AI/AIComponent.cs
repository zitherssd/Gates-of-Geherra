using Assets.Scripts.Battle.Components.AI.Behaviors;
using System.Collections.Generic;

namespace Assets.Scripts.Battle.Components.AI
{
    public enum AiRuleset { DEFAULT, OldMan, Ninja, Maniac }
    public enum AIState { Thinking, Acting, Moving }

    public class AIComponent
    {
        private readonly Actor actor;
        private readonly AiRuleset ruleset;
        private readonly List<IAIBehavior> behaviors = new();

        public AIState currentState;

        public AIComponent(Actor actor, AiRuleset ruleset = AiRuleset.DEFAULT)
        {
            this.actor = actor;
            this.ruleset = actor.ActorData.ruleset;

            switch (ruleset)
            {
                case AiRuleset.DEFAULT:
                    behaviors.Add(new AttackBehavior());
                    behaviors.Add(new ReactionBehavior());
                    behaviors.Add(new DashBehavior());
                    behaviors.Add(new ApproachBehavior());
                    break;
                case AiRuleset.OldMan:
                    behaviors.Add(new AttackBehavior());
                    behaviors.Add(new ReactionBehavior());
                    behaviors.Add(new DashBehavior());
                    behaviors.Add(new ApproachBehavior());
                    break;
                case AiRuleset.Ninja:
                    break;
                case AiRuleset.Maniac:
                    break;
            }
        }


        public void Update()
        {
            if (actor.isControllable()) return;

            if (actor.state.CurrentState == actor.state.moveState)
            {
                actor.state.moveState.action.Direction = actor.target.DirectionToClosestEnemy;
                if (actor.target.DistanceToClosestEnemy < 1.4f) actor.state.moveState.action.cancel?.Invoke();
            }
            if (actor.state.IsIdle())
                foreach (var behavior in behaviors)
                {
                    if (behavior.Execute(this, actor)) return;
                }
        }
    }
}