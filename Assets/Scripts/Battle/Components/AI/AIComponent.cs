using Assets.Scripts.Actions;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.BaseAction;

namespace Assets.Scripts.Battle.Components.AI
{
    public enum AiRuleset { DEFAULT, Boxer }

    public class AIComponent
    {

        //Boxer AIComponent

        private readonly Actor actor;
        private readonly AiRuleset ruleset;

        public AIComponent(Actor actor)
        {
            this.actor = actor;
        }

        public void ChooseAction(Action<BaseAction> onActionChosen)
        {
            if (actor.isControllable())
            {
                var AvaliableSkills = actor.ActorData.actions;
                Time.timeScale = 0f; UIManager.GetInstance().ShowUI();
                UIManager.GetInstance().DrawActionsAndWaitForSelectionOrNull(AvaliableSkills, selectedSkill =>
                {
                    if (selectedSkill == null)
                    {
                        Time.timeScale = 1f; UIManager.GetInstance().HideUI();
                        Debug.Log("Player skipped");
                        onActionChosen.Invoke(null);
                        return;
                    }
                    else
                    {

                        Time.timeScale = 1f; UIManager.GetInstance().HideUI();
                        onActionChosen.Invoke(selectedSkill);
                        return;
                    }
                });
            }
            else
            {
                switch (ruleset)
                {
                    case AiRuleset.DEFAULT:
                        EvaluateDefaultRuleset((obj) => onActionChosen.Invoke(obj));
                        break;
                    case AiRuleset.Boxer:
                        //onActionChosen.Invoke(EvaluateBoxerRuleset());
                        break;
                    default:
                        EvaluateDefaultRuleset((obj) => onActionChosen.Invoke(obj));
                        break;
                }
            }
        }

        public void ChooseReaction(Action<BaseReaction> onReactionChosen, BaseAction incomingAction)
        {
            if (actor.isControllable())
            {
                Time.timeScale = 0f; UIManager.GetInstance().ShowUI();
                var reactionsAsActions = new List<BaseAction>();
                {
                    foreach (var reaction in actor.ActorData.reactions)
                    {
                        reactionsAsActions.Add(reaction as BaseAction);
                    }
                }
                UIManager.GetInstance().DrawActionsAndWaitForSelectionOrNull(reactionsAsActions, selectedReaction =>
                {
                    Time.timeScale = 1f; UIManager.GetInstance().HideUI();
                    if (selectedReaction != null && selectedReaction != BaseReaction.NoReaction)
                    {
                        if (selectedReaction.Tags.Contains(TAG.KILL_TRACKING) && incomingAction is AttackSkill)
                        {
                            var attackskill = incomingAction as AttackSkill;
                        }

                        onReactionChosen.Invoke(selectedReaction as BaseReaction);
                    }
                });
            }
            else
            {
                switch (ruleset)
                {
                    default:
                        onReactionChosen.Invoke(EvaluateDefaultReactionRuleset());
                        break;
                }
            }
        }

        public BaseAction EvaluateBoxerRuleset()
        {
            return null;
        }

        public void EvaluateDefaultRuleset(Action<BaseAction> action)
        {
            var chosenSkill = ChooseValidSkillInRangeOrReturnNull();
            if (chosenSkill == null)
            {
                //must move
                var moveSkill = actor.ActorData.actions.OfType<MoveAction>().First();
                moveSkill.Direction = actor.target.DirectionToClosestEnemy;
                action.Invoke(moveSkill);
            }
            else
            {
                actor.StartCoroutine(actor.WaitForTime(() => {
                    var directionToEnemy = actor.target.DirectionToClosestEnemy;
                    var perpendicular = Vector3.Cross(Vector3.up, directionToEnemy).normalized;
                    perpendicular *= UnityEngine.Random.Range(-0.3f, 0.3f);
                    var dirleftvector = new Vector3(perpendicular.x, 0, perpendicular.z);
                    chosenSkill.Direction = new Vector3(directionToEnemy.x, 0, directionToEnemy.z) + dirleftvector;
                    action.Invoke(chosenSkill);
                }, UnityEngine.Random.Range(0.2f, 1f)));
            }
        }

        public BaseReaction EvaluateDefaultReactionRuleset()
        {
            return null;
        }




        public List<BaseAction> GetValidContinueComboSkills()
        {
            return actor.ActorData.actions.Where(skill => skill.IsValid(actor, out _) && !skill.Tags.Contains(TAG.STARTER)).ToList();
        }
        public List<BaseAction> GetValidStartComboSkills()
        {
            return actor.ActorData.actions.Where(skill => skill.IsValid(actor, out _)).ToList();
        }
        public BaseAction ChooseValidSkillInRangeOrReturnNull()
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