using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.AI.Behaviors
{
    public class AttackBehavior : IAIBehavior
    {
        private float delayTimer = 0f;
        private float delayDuration = 0f;
        private bool isDelaying = true;

        public bool Execute(AIComponent ai, Actor actor)
        {
            if (isDelaying)
            {
                delayTimer += Time.deltaTime;
                if (delayTimer >= delayDuration)
                {
                    isDelaying = false;
                    delayTimer = 0f;

                    // Choose and execute the skill after delay
                    var skill = ChooseValidSkillInRange(actor);
                    if (skill == null) return false;

                    var direction = actor.target.DirectionToClosestEnemy;
                    skill.Direction = new Vector3(direction.x, 0, direction.z);

                    delayDuration = UnityEngine.Random.Range(0.05f, 0.3f);
                    isDelaying = true;

                    actor.UseAction(skill, actor.state.TransitionToIdle);
                    return true;
                }

                return false; // Still delaying
            }
            isDelaying = true;
            return false;
        }

        private BaseAction ChooseValidSkillInRange(Actor actor)
        {
            var skills = actor.ActorData.actions
                .Where(skill => skill.IsValidAndInRange(actor))
                .ToList();

            return skills.Any() ? skills[UnityEngine.Random.Range(0, skills.Count)] : null;
        }
    }
}
