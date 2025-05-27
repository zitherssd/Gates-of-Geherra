using Assets.Scripts.Battle.Actor.AI.Behaviors;
using Assets.Scripts.Battle.Actor.AI.Conditions;
using System.Collections.Generic;

namespace Assets.Scripts.Battle.Actor.AI
{
    public enum AiRuleset { DEFAULT, OldMan, Ninja, Maniac, Hungry, ShurkienThrower }
    public enum AIState { Thinking, Acting, Moving }

    public class AIBT
    {
        private readonly Actor actor;
        private List<BTNode> behaviors = new List<BTNode>();
        private float aiTickCooldown = 0.05f;
        private float aiTickTimer = 0f;

        public AIState currentState;

        public AIBT(Actor actor)
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
                        behaviors = OldManBehavior;
                        break;
                    case AiRuleset.OldMan:
                        behaviors = OldManBehavior;
                        break;
                    case AiRuleset.Hungry:
                        behaviors = OldManBehavior;
                        break;
                    case AiRuleset.Maniac:
                        behaviors = EngragedManiac;
                        break;
                    case AiRuleset.Ninja:
                        break;
                    case AiRuleset.ShurkienThrower:
                        behaviors = ShurkienThrowerBehavior;
                        break;
                }
            }
        }

        private static readonly List<BTNode> EngragedManiac = new List<BTNode>
        {
            new SequenceNode(new List<BTNode>
            {
                new InsideEnemyHitbox(),
                new DodgeBehavior(.5f),
            }),
            new SequenceNode(new List<BTNode>
            {
                new AttackWithValidSkill(),
            }),
            new SequenceNode(new List<BTNode>
            {
                new StaminaLesserThan(0.5f),
                new DistanceToPlayerSmallerThan(3f),
                new MoveAwayFromPlayer(.5f),
            }),
            new SequenceNode(new List<BTNode>
            {
                new DistanceToPlayerGreaterThan(1.5f),
                new MoveTowardsPlayer(.5f)
            }),
        };


        private static readonly List<BTNode> OldManBehavior = new List<BTNode>
        {
            new SequenceNode(new List<BTNode>
            {
                new InsideEnemyHitbox(),
                new BlockBehavior(.75f),
            }),
            new SequenceNode(new List<BTNode>
            {
                new AttackWithValidSkill(),
            }),
            new SequenceNode(new List<BTNode>
            {
                new InsideEnemyHitbox(),
                new DodgeBehavior(1f),
            }),
            new ApproachBehavior(),
        };

        private static readonly List<BTNode> ShurkienThrowerBehavior = new List<BTNode>
        {
            new SequenceNode(new List<BTNode>
            {
                new ConditionNode(actor => actor.target.DistanceToClosestEnemy > 3 && actor.target.DistanceToClosestEnemy < 5),
                new AttackWithValidSkill(),
            }),
            new SequenceNode(new List<BTNode>
            {
                new DistanceToPlayerSmallerThan(3),
                new MoveAwayFromPlayer(0.2f),
            }),

            new SequenceNode(new List<BTNode>
            {
                new DistanceToPlayerGreaterThan(5),
                new MoveTowardsPlayer(0.2f)
            }),
        };

        public void Update()
        {
            if (actor.isControllable) return;

            //aiTickTimer -= Time.deltaTime;
            //if (aiTickTimer > 0) return;
            //aiTickTimer = aiTickCooldown;

            if (actor.state.IsIdle() || actor.state.CurrentState == actor.state.moveState)
            {
                foreach (var behavior in behaviors)
                {
                    if (behavior.Execute(this, actor) == NodeState.Sucess)
                    {
                        return; //If the behavior return true, stop executing other behaviors
                    }
                }
            }
        }


    }
}