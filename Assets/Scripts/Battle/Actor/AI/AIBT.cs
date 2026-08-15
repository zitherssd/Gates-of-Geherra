using Assets.Scripts.Battle.Actor.AI.Behaviors;
using Assets.Scripts.Battle.Actor.AI.Conditions;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI
{
    public enum AiRuleset { DEFAULT, OldMan, Ninja, Maniac, Hungry, ShurkienThrower, TacticalFlanker, SmartApproach, Hulk }
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
            if (actor.Definition != null)
            {
                switch (actor.Definition.AIRuleset)
                {
                    case AiRuleset.DEFAULT:
                        behaviors = SandboxGuy;
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
                    case AiRuleset.TacticalFlanker:
                        behaviors = TacticalFlankerBehavior;
                        break;
                    case AiRuleset.SmartApproach:
                        behaviors = SmartApproachBehaviorTree;
                        break;
                    case AiRuleset.Hulk:
                        behaviors = HulkBehavior;
                        break;
                }
            }
        }

        private static readonly List<BTNode> SandboxGuy = new List<BTNode>
        {
            new SequenceNode(new List<BTNode>
            {
                new MoveAwayFromLevel(0.3f,2f)
            }),
        };


        private static readonly List<BTNode> EngragedManiac = new List<BTNode>
        {
            // Always able to dodge out of danger, regardless of stamina.
            new SequenceNode(new List<BTNode>
            {
                new InsideEnemyHitbox(),
                new DodgeBehavior(.5f),
            }),
            // Tired: stamina below 25% — backs off to recover and refuses to fight
            // again until stamina is fully restored to 100%.
            new SequenceNode(new List<BTNode>
            {
                new StaminaLesserThan(0.25f),
                new DistanceToPlayerSmallerThan(3f),
                new MoveAwayFromPlayer(1.5f),
            }),
            // Fights only when stamina is full (100%).
            new SequenceNode(new List<BTNode>
            {
                new StaminaGreaterThan(0.99f),
                new AttackWithValidSkill(),
            }),
            // Chases the player only when stamina is full (100%).
            new SequenceNode(new List<BTNode>
            {
                new StaminaGreaterThan(0.99f),
                new DistanceToPlayerGreaterThan(1.5f),
                new MoveTowardsPlayer(.5f),
            }),
        };

        /// <summary>
        /// Hulk: slow heavy brute with NO defensive options — attacks whenever a
        /// skill is in range, otherwise approaches the player.
        /// </summary>
        private static readonly List<BTNode> HulkBehavior = new List<BTNode>
        {
            new SequenceNode(new List<BTNode>
            {
                new AttackWithValidSkill(),
            }),
            new SequenceNode(new List<BTNode>
            {
                new DistanceToPlayerSmallerThan(6f),
                new ApproachBehavior(),
            }),
            new ApproachBehavior(),
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
            new SequenceNode(new List<BTNode>
            {
                new DistanceToPlayerSmallerThan(6.0f),
                new ApproachBehavior(),
            }),
            new ApproachBehavior(),
        };

        private static readonly List<BTNode> ShurkienThrowerBehavior = new List<BTNode>
        {
            new SequenceNode(new List<BTNode>
            {
                new ConditionNode(actor => actor.target.DistanceToClosestEnemy > 3 && actor.target.DistanceToClosestEnemy < 5),
                new SelectorNode(new List<BTNode>
                {
                    // Throw only when no friendly is in the way...
                    new SequenceNode(new List<BTNode>
                    {
                        new LineOfFireClear(),
                        new AttackWithValidProjectileSkill(),
                    }),
                    // ...otherwise sidestep laterally to find a clear angle of fire.
                    new RepositionForClearShot(),
                }),
            }),
            new SequenceNode(new List<BTNode>
            {
                new DistanceToPlayerSmallerThan(3),

                new SelectorNode(new List<BTNode>
                {
                    new MoveAwayFromPlayer(3, 3),
                    new AttackWithValidSkill() 
                })
            }),

            new SequenceNode(new List<BTNode>
            {
                new DistanceToPlayerGreaterThan(5),
                new MoveTowardsPlayer(0.2f)
            }),
        };

        private static readonly List<BTNode> TacticalFlankerBehavior = new List<BTNode>
        {
            new SequenceNode(new List<BTNode>
            {
                new InsideEnemyHitbox(),
                new BlockBehavior(1f),
            }),
            new SequenceNode(new List<BTNode>
            {
                new AttackWithValidSkill(),
            }),
            new SequenceNode(new List<BTNode>
            {
                new DistanceToPlayerSmallerThan(6.0f),
                new CircleApproachBehavior(),
            }),
            new ApproachBehavior(),
        };

        /// <summary>
        /// Smart Approach behavior tree: enemies spread out to surround the player
        /// instead of all beelining together. Uses SmartApproachBehavior for
        /// crowd-aware encirclement, with standard dodge, block, and attack nodes.
        /// Falls back to direct ApproachBehavior so enemies always push in.
        /// </summary>
        private static readonly List<BTNode> SmartApproachBehaviorTree = new List<BTNode>
        {
            new SequenceNode(new List<BTNode>
            {
                new InsideEnemyHitbox(),
                new DodgeBehavior(0.5f),
            }),
            new SequenceNode(new List<BTNode>
            {
                new InsideEnemyHitbox(),
                new BlockBehavior(0.75f),
            }),
            new SequenceNode(new List<BTNode>
            {
                new AttackWithValidSkill(),
            }),
            new SmartApproachBehavior(),
            new ApproachBehavior(),
        };

        public void Update()
        {
            if (actor.isControllable) return;
            if (actor.Runtime.isDead() || actor.state.IsStaggered()) return;
            aiTickTimer -= Time.deltaTime;
            if (aiTickTimer > 0) return;
            aiTickTimer = aiTickCooldown;

            if (actor.state.IsIdle() || actor.state.IsMoving(out _))
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