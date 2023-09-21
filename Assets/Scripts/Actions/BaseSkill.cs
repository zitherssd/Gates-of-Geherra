using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Assets.BaseActorBattler;

namespace Assets.Scripts.Actions
{
    [CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skill", order = 1)]
    public class BaseSkill : BaseAction
    {

        public ANIMATIONTYPE AnimationType;
        public ATTACKTYPE AttackType;
        public TARGETINGOPTION TargetOption;
        public List<SKILLTAG> Tags;
        public float Range;
        public float Damage;
        public float PostureDamage;
        public float KnockbackForce;



        public void Perform(BaseActorBattler casterActor, BaseActorBattler enemyActor, BaseSkill skill, BaseReaction reaction, Action onPerformEnd)
        {
            //ONLY FOR TARGETINGOPTION.ENEMY SKILLS
            if (skill.Tags.Contains(SKILLTAG.PROJECTILE))
            {

            }


            if (skill.Tags.Contains(SKILLTAG.MOVE_NEAR_ENEMY_BEFORE_ATTACK))
            {
                var targetVector = enemyActor.transform.position - casterActor.transform.position;

                if (!(targetVector.magnitude < 2f))
                {
                    var minimumDistance = targetVector.normalized;
                    casterActor.MoveToPosition(casterActor.transform.position + targetVector - minimumDistance, State.Move, () => {
                        casterActor.PlayAnimation(skill.AnimationType.ToString(), () => 
                        {
                            onPerformEnd();
                            return;
                        });
                    });
                }
                else casterActor.PlayAnimation(skill.AnimationType.ToString(), () =>
                {
                    onPerformEnd();
                    return;
                });
            };
        }

        public void Perform(BaseActorBattler casterActor, BaseSkill skill, Action onPerformEnd)
        {
            //ONLY FOR TARGETINGOPTION.SELF SKILLS
            onPerformEnd();
        }

        public void WarmUp(BaseActorBattler casterActor, Action onWarmUpEnd)
        {
            var texttobeshown = $"{casterActor.name} is using { this.Name }!";
            UIManager.GetInstance().SetTextThenFade(texttobeshown, 1f);

            if (AttackType == ATTACKTYPE.Projectile)
            {
                //Summon projectile
            }
            
            
            
            onWarmUpEnd();
            //Play an animation before moving,
            //Show skill name,
            //Certain special effects
        }

        public override bool IsValid(BaseActorBattler caster, out string InvalidReason)
        {
            InvalidReason = "";
            if (!HasUsesLeft()) return false;
            
            if (Range != 0)
            {
                //Get all active 
                var potentialTargets = BattleManager.GetInstance().EnemyActors;
                //If any of them are in range return true
                if (potentialTargets.Any(target => (target.transform.position - caster.transform.position).magnitude < Range))
                    return true;
                else
                {
                    InvalidReason = "too far";
                    return false;
                }
            }

            return true;
        }

        public override bool IsValid(BaseActorBattler caster)
        {
            if (!HasUsesLeft()) return false;

            if (Range != 0)
            {
                var potentialTargets = new List<BaseActorBattler>();

                //Get all active
                if (caster.isControllable())
                    potentialTargets = BattleManager.GetInstance().EnemyActors;
                else
                    potentialTargets = BattleManager.GetInstance().PlayerActors;
                //If any of them are in range return true
                if (potentialTargets.Any(target => (target.transform.position - caster.transform.position).magnitude < Range))
                    return true;
                else
                {
                    return false;
                }
            }

            return true;
        }
    }

    public enum ANIMATIONTYPE { NONE, Punch, Kick, Shuriken, Highkick }
    public enum ATTACKTYPE { Attack, Projectile }
    public enum TARGETINGOPTION { SELF, ENEMY }
    public enum SKILLTAG { MOVE_NEAR_ENEMY_BEFORE_ATTACK, PROJECTILE, KNOCKBACK_AIR }


}