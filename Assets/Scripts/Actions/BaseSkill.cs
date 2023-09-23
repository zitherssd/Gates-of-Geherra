using System;
using System.Collections.Generic;
using System.Linq;
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



        public void Perform(BaseActorBattler casterActor, BaseActorBattler enemyActor, BaseReaction reaction, Action onPerformEnd)
        {
            //Performed used for single Target skills
            if (this.Tags.Contains(SKILLTAG.MOVE_OFFSET_BEHIND) || this.Tags.Contains(SKILLTAG.MOVE_OFFSET_INFRONT))
            {


                var targetVector = enemyActor.transform.position - casterActor.transform.position;

                if (this.Tags.Contains(SKILLTAG.MOVE_OFFSET_BEHIND)) targetVector += Vector3.Cross(targetVector, Vector3.up).normalized;
                if (this.Tags.Contains(SKILLTAG.MOVE_OFFSET_INFRONT)) targetVector += Vector3.Cross(targetVector, -Vector3.up).normalized;

                casterActor.MoveToPosition(casterActor.transform.position + targetVector, MoveState.Slerp, () => { });


            }

            casterActor.PlayAnimation(this.AnimationType.ToString(), () =>
            {
                onPerformEnd();
                return;
            });

        }

        public void Perform(BaseActorBattler casterActor, BaseSkill skill, Action onPerformEnd)
        {
            //ONLY FOR TARGETINGOPTION.SELF SKILLS
            onPerformEnd();
        }

        public void WarmUp(BaseActorBattler casterActor, BaseActorBattler targetActor, Action onWarmUpEnd)
        {
            if (this.Tags.Contains(SKILLTAG.MOVE_NEAR_ENEMY_BEFORE_ATTACK))
            {

                var targetVector = targetActor.transform.position - casterActor.transform.position;

                if (!(targetVector.magnitude < 1f))
                {
                    var minimumDistance = targetVector.normalized * 0.66f;
                    casterActor.MoveToPosition(casterActor.transform.position + targetVector - minimumDistance, MoveState.Move, onWarmUpEnd);
                    return;
                }
            };

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

    public enum ANIMATIONTYPE { NONE, Punch, Kick, Shuriken, Highkick, PalmStrike }
    public enum ATTACKTYPE { Attack, Projectile }
    public enum TARGETINGOPTION { SELF, ENEMY }
    public enum SKILLTAG { MOVE_NEAR_ENEMY_BEFORE_ATTACK, PROJECTILE, KNOCKBACK_AIR, MOVE_OFFSET_BEHIND, MOVE_OFFSET_INFRONT }


}