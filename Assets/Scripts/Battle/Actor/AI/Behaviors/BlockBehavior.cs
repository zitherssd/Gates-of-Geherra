using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Reactions;
using Assets.Scripts.Battle.Actions.Skills;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors

{
    public class BlockBehavior : BTNode
    {
        private float ChanceToBlock;
        private AttackSkill lastAttack;
        public BlockBehavior(float chanceToBlock)
        {
            ChanceToBlock = chanceToBlock;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            AttackSkill incomingAttack;
            actor.target.ClosestEnemy.state.IsAttacking(out incomingAttack);
            if (incomingAttack != lastAttack)
            {
                lastAttack = incomingAttack;
                if (UnityEngine.Random.value < ChanceToBlock)
                {
                    var block = GetBlock(actor);

                    if (block)
                    {
                        actor.UseAction(block);
                        return NodeState.Sucess;
                    }
                }
            }
            return NodeState.Failure;
        }


        private BaseAction GetBlock(Actor actor)
        {
            var randomBlock = actor.Runtime.actions.OfType<Block>().Where(action => action.IsValid(actor, out _)).OrderBy(x => UnityEngine.Random.value).FirstOrDefault();

            if (randomBlock != null)
            {
                return randomBlock;
            }
            return null;
        }
    }
}
