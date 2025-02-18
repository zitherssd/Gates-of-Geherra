using Assets.Scripts.Battle.Actions.Reactions;
using Assets.Scripts.Battle.Actions.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.AI.Behaviors
{
    public class AttackBehavior : IAIBehavior
    {
        private float cooldownTimer = 0f;
        private float reactionTimer = 0f;

        public bool Execute(AISystem ai, Actor actor)
        {
            // Check if in range for attack
            var skill = ChooseValidSkillInRange(actor);
            if (skill != null)
            {
                var direction = actor.target.DirectionToClosestEnemy;
                skill.Direction = new Vector3(direction.x, 0, direction.z);
                if(reactionTimer <= 0)
                    reactionTimer = UnityEngine.Random.Range(0.2f, 0.3f);
            }
            else
            reactionTimer += Time.deltaTime;



            // If reactionTimer is still running, decrease it
            if (reactionTimer > 0)
            {
                reactionTimer -= Time.deltaTime;
                if (reactionTimer <= 0)
                {
                    // Time to react!
                    skill = ChooseValidSkillInRange(actor);
                    if (skill != null)
                    {
                        actor.UseAction(skill, actor.state.TransitionToIdle);
                        return true;
                    }
                }
            }
            return false;
        }

        private BaseAction ChooseValidSkillInRange(Actor actor)
        {
            return actor.ActorData.actions.Where(skill => skill.IsValidAndInRange(actor)).OrderBy(x => UnityEngine.Random.value).FirstOrDefault();
        }
    }
}
