using Assets.Scripts.Battle.Actions.Reactions;
using Assets.Scripts.Battle.Actions.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.AI.Behaviors
{
    public class ReactionBehavior : IAIBehavior
    {
        private float cooldown = 0f;
        private BaseAction pendingAction = null;

        public bool Execute(AIComponent ai, Actor actor)
        {
            // If there's a pending action, update the timer
            if (pendingAction != null)
            {
                cooldown -= Time.deltaTime;
                if (cooldown <= 0f)
                {
                    actor.UseAction(pendingAction, actor.state.TransitionToIdle);
                    pendingAction = null; // Reset after executing the action
                    return true;
                }
                return true; // Still waiting for the delay to finish
            }

            // Check for an incoming attack
            AttackSkill incomingAttack;
            if (actor.target.ClosestEnemy.state.IsAttacking(out incomingAttack))
            {
                if ((incomingAttack.Range + 1 > actor.target.DistanceToClosestEnemy))
                {
                    var reaction = ChooseReaction(actor, incomingAttack);
                    if (reaction != null)
                    {
                        // Set up the delay and store the action
                        cooldown = UnityEngine.Random.Range(0f, 0.05f);
                        pendingAction = reaction;
                        if(incomingAttack.Tags.Contains(BaseAction.TAG.TECH))

                        return true; // Start the delay
                    }
                }
            }

            return false;
        }

        private BaseAction ChooseReaction(Actor actor, BaseAction incomingAction)
        {
            var reactions = actor.ActorData.actions
                .Where(action => action is Block || action is Dodge && action.IsValid(actor, out _))
                .ToList();

            if (!reactions.Any()) return null;

            var chosenReaction = reactions[UnityEngine.Random.Range(0, reactions.Count)];
            if (chosenReaction is Dodge dodge)
            {
                dodge.Direction = Vector3.Cross(actor.target.DirectionToClosestEnemy, Vector3.up);
            }

            return chosenReaction;
        }
    }

}
