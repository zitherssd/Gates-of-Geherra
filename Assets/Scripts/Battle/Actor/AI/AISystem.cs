using Assets.Scripts.Battle.Components.AI.Behaviors;
using Assets.Scripts.Battle.Components.State;
using System.Collections.Generic;

namespace Assets.Scripts.Battle.Components.AI
{
    public enum AiRuleset { DEFAULT, OldMan, Ninja, Maniac, Hungry }
    public enum AIState { Thinking, Acting, Moving }

    public class AISystem
    {
        private readonly Actor actor;
        private readonly List<IAIBehavior> behaviors = new();

        public AIState currentState;

        public AISystem(Actor actor)
        {
            this.actor = actor;
            actor.OnReset += Reset;
            Reset();
        }

        public void Reset()
        {
            if (actor.ActorData)
            {
                switch (actor.ActorData.AIRuleset)
                {
                    case AiRuleset.DEFAULT:
                        behaviors.Add(new AttackBehavior());
                        behaviors.Add(new ReactionBehavior());
                        behaviors.Add(new DashBehavior());
                        behaviors.Add(new ApproachBehavior());
                        break;
                    case AiRuleset.OldMan:
                        behaviors.Add(new AttackBehavior());
                        behaviors.Add(new BlockBehavior(0.5f));
                        behaviors.Add(new GroupFlankBehavior());
                        break;
                    case AiRuleset.Hungry:
                        behaviors.Add(new AttackBehavior());
                        behaviors.Add(new ReactionBehavior());
                        behaviors.Add(new GroupFlankBehavior());
                        break;
                    case AiRuleset.Maniac:
                        behaviors.Add(new AttackBehavior());
                        behaviors.Add(new BlockBehavior(1f));
                        behaviors.Add(new ApproachBehavior());

                        break;
                }
            }
        }


        public void Update()
        {
            if (actor.isControllable) return;

            if (actor.state.CurrentState == actor.state.moveState)
            {
                actor.state.moveState.action.Direction = actor.target.DirectionToClosestEnemy;
                if (actor.target.DistanceToClosestEnemy < 1.4f) actor.state.moveState.action.OnCancel?.Invoke();
            }
            if (actor.state.IsIdle())
                foreach (var behavior in behaviors)
                {
                    if (behavior.Execute(this, actor)) return;
                }
        }
    }
}