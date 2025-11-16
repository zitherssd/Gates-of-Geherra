using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Reactions;
using Assets.Scripts.Battle.Actions.Skills;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class DodgeBehavior : BTNode
    {
        float chance;
        private AttackSkill lastAttack;

        public DodgeBehavior(float chanceToDodge)
        {
            this.chance = chanceToDodge;

        }
        public override NodeState Execute(AIBT ai, Actor actor)
        {
            // Check for an incoming attack
            AttackSkill incomingAttack;
            actor.target.ClosestEnemy.state.IsAttacking(out incomingAttack);
            if (incomingAttack != lastAttack)
            {
                lastAttack = incomingAttack;
                if (UnityEngine.Random.value < chance)
                {
                    var reaction = ChooseDodge(actor, incomingAttack);
                    if (reaction != null)
                    {
                        actor.UseAction(reaction, actor.state.TransitionToIdle);

                        return NodeState.Sucess;
                    }
                }
            }
            return NodeState.Failure;
        }

        private BaseAction ChooseDodge(Actor actor, BaseAction incomingAction)
        {
            var randomDodge = actor.ActorData.actions.OfType<Dodge>().Where(action => action.IsValid(actor, out _)).OrderBy(x => UnityEngine.Random.value).FirstOrDefault();

            if (randomDodge != null)
            {

                randomDodge.Direction = -(actor.target.DirectionToClosestEnemy + Vector3.Cross(actor.target.DirectionToClosestEnemy, Vector3.up) * UnityEngine.Random.Range(-1f, 1f)).normalized;
                return randomDodge;
            }
            return null;
        }

    }
}


