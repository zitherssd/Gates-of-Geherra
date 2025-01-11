using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Reactions;
using Assets.Scripts.Battle.Actions.Skills;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.BaseAction;

namespace Assets.Scripts.Battle.Components.AI
{
    public enum AiRuleset { DEFAULT, Boxer }

    public enum AIState { Thinking }

    public class AIComponent
    {

        //Boxer AIComponent

        private readonly Actor actor;
        private readonly AiRuleset ruleset;
        private Actor target;
        private AIState aiState;
        private float timer = 0f;
        private float timeToThink;
        private float reactionCooldown;

        public AIComponent(Actor actor)
        {
            timeToThink = 0.5f;
            this.actor = actor;
        }

        public void SetTarget(Actor target)
        {
            this.target = target;
        }

        public BaseAction ChoseReaction(BaseAction incomingAction)
        {
            var validReactions = new List<BaseAction>();

            // Add available actions to the list
            var blocks = actor.ActorData.actions.OfType<Block>().ToList();
            if (blocks.Any())
                validReactions.AddRange(blocks);

            var dodges = actor.ActorData.actions.OfType<Dodge>().ToList();
            if (dodges.Any())
                validReactions.AddRange(dodges);

            // Ensure validReactions is not empty
            if (validReactions.Count == 0)
            {
                Debug.LogWarning("No valid reactions found!");
                return null;  // No valid reactions, return null
            }

            // Choose a random valid reaction
            int randomIndex = UnityEngine.Random.Range(0, validReactions.Count);
            var chosenReaction = validReactions[randomIndex];

            // Set direction if the chosen action is a Dodge
            if (chosenReaction is Dodge)
            {
                chosenReaction.Direction = Vector3.Cross(actor.target.DirectionToClosestEnemy, Vector3.up);
            }

            return chosenReaction;
        }

        internal void Update()
        {
            if (actor.isControllable()) return;
            if (actor.state.CurrentState == actor.state.idleState)
            {
                if (Reaction()) return;
                if (Dash()) return;
                if (Attack()) return;
                if (Approach()) return;

                timer += Time.deltaTime;
                reactionCooldown += Time.deltaTime;
            }
            if (actor.state.CurrentState == actor.state.actingState && actor.state.actingState.action is AttackSkill)
            {
                var skill = actor.state.actingState.action as AttackSkill;
                if (skill.state == AttackSkill.STATE.recovery)
                    if (Reaction()) return;
            }

            if (actor.state.CurrentState == actor.state.moveState)
            {
                actor.state.moveState.action.Direction = actor.target.DirectionToClosestEnemy;
                if (actor.target.DistanceToClosestEnemy < 1.4f) actor.state.moveState.action.cancel?.Invoke();
            }
            //anticipate
            //while anticipating it can react to your attacks;
            //wait a bit and then think()
            //>if very far wait a bit and approach 1/3 of a distance
            //>if close range
        }


        private bool Reaction()
        {
            //if (reactionCooldown > 3f)
            //{
            //    //REACTION
            //    if (target.state.CurrentState == target.state.actingState && target.state.actingState.action is AttackSkill) //this means that if enemy is attacking with an attackskill
            //    {
            //        var incomingAttack = target.state.actingState.action as AttackSkill;
            //        if (incomingAttack.SelfForce + incomingAttack.Range + target.rigidbody.velocity.magnitude > (target.transform.position - actor.transform.position).magnitude) //if in range
            //        {
            //            if (incomingAttack.state == AttackSkill.STATE.windup) //if in windup
            //            {
            //                var reaction = ChoseReaction(incomingAttack);
            //                if (reaction != null)
            //                {
            //                    reactionCooldown = 0f;
            //                    timeToThink = 0f;
            //                    UseActionThenReturnToIdle(reaction);
            //                    return true;
            //                }
            //            }
            //        }
            //    }
            //}

            return false;
        }
        private bool Dash()
        {
            if (!actor.ActorData.actions.OfType<Charge>().Where(action => action.IsValid(actor, out _)).Any()) return false; //if there isn't any action of type charge that is valid
            if (actor.rigidbody.velocity.magnitude > 0.5f) return false; //if already moving
            if (actor.target.DistanceToClosestEnemy < 2.5f) return false; //if to close
            var chargeSKill = actor.ActorData.actions.OfType<Charge>().First(); //get
            timeToThink = 0.2f;
            actor.UseAction(chargeSKill, () =>
            {
                actor.state.TransitionTo(actor.state.idleState);
            });
            return true;
        }
        private bool Attack()
        {
                var chosenSkill = ChooseValidSkillInRange();
                if (chosenSkill == null) return false;
                if (!chosenSkill.IsValid(actor, out _)) return false;
                var directionToEnemy = actor.target.DirectionToClosestEnemy;
                var perpendicular = Vector3.Cross(Vector3.up, directionToEnemy).normalized;
                perpendicular *= UnityEngine.Random.Range(-0.3f, 0.3f);
                var dirleftvector = new Vector3(perpendicular.x, 0, perpendicular.z);
                chosenSkill.Direction = new Vector3(directionToEnemy.x, 0, directionToEnemy.z) + dirleftvector;
                timeToThink = 1f;
                UseActionThenReturnToIdle(chosenSkill);
                return true;
        }
        private bool Approach()
        {
            if (actor.rigidbody.velocity.magnitude > 0.5f) return false;
            if (actor.target.DistanceToClosestEnemy < 1.4f)
                return false;
            else
            {
                var moveSkill = actor.ActorData.actions.OfType<MoveAction>().First();
                if (!moveSkill.IsValid(actor, out _)) return false;
                moveSkill.Direction = actor.target.DirectionToClosestEnemy;
                timeToThink = 0.4f;
                UseActionThenReturnToIdle(moveSkill);
                return true;
            }
        }
        private void UseActionThenReturnToIdle(BaseAction action)
        {
            actor.UseAction(action, () =>
            {
                actor.state.TransitionTo(actor.state.idleState);
            });
            timer = 0f;
        }

        public List<BaseAction> GetValidContinueComboSkills()
        {
            return actor.ActorData.actions.Where(skill => skill.IsValid(actor, out _) && !skill.Tags.Contains(TAG.STARTER)).ToList();
        }
        public List<BaseAction> GetValidStartComboSkills()
        {
            return actor.ActorData.actions.Where(skill => skill.IsValid(actor, out _)).ToList();
        }
        public BaseAction ChooseValidSkillInRange()
        {
            List<BaseAction> validskills = new();

            validskills = actor.ActorData.actions.Where(skills => skills.IsValidAndInRange(actor)).ToList();

            if (validskills.Count == 0) return null;

            var random = new System.Random();
            var index = random.Next(validskills.Count);

            return validskills[index];
        }
    }
}