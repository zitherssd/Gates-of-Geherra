using System.Linq;
using Assets.Scripts.Battle.Actions;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class AttackWithValidSkill : BTNode
    {
        private float cooldownTimer = 0f;
        private float reactionTimer = 0f;

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            // Check if in range for attack
            var skill = ChooseValidSkillInRange(actor);
            if (skill != null)
            {
                var direction = actor.target.DirectionToClosestEnemy;
                actor.movement.FaceDirection(actor.target.DirectionToClosestEnemy);
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
                        skill.Direction = actor.target.DirectionToClosestEnemy;
                        actor.UseAction(skill, actor.state.TransitionToIdle);
                        return NodeState.Sucess;
                    }
                }
            }
            return NodeState.Failure;
        }

        private BaseAction ChooseValidSkillInRange(Actor actor)
        {
            return actor.ActorData.actions.Where(skill => skill.IsValidAndInRange(actor)).OrderBy(x => UnityEngine.Random.value).FirstOrDefault();
        }
    }
}
