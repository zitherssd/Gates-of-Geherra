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
        public SKILLTYPE SkillType;
        public TARGETINGOPTION TargetOption;
        public List<SKILLTAG> Tags;
        public float Range;
        public float Damage;
        public float PostureDamage;
        public float KnockbackForce;
        private Vector3 targetPoint;


        public void PerformTarget(BaseActorBattler casterActor, BaseActorBattler enemyActor, BaseReaction reaction, Action onPerformEnd)
        {
            //Perform move offset then play animation
            if (this.Tags.Contains(SKILLTAG.MOVE_OFFSET_BEHIND) || this.Tags.Contains(SKILLTAG.MOVE_OFFSET_INFRONT))
            {
                var targetVector = enemyActor.transform.position - casterActor.transform.position;
                
                if (this.Tags.Contains(SKILLTAG.MOVE_OFFSET_BEHIND)) targetVector += Vector3.Cross(targetVector, Vector3.up);
                if (this.Tags.Contains(SKILLTAG.MOVE_OFFSET_INFRONT)) targetVector += Vector3.Cross(targetVector, -Vector3.up);

                casterActor.MoveToPosition(casterActor.transform.position + targetVector, MoveState.Move, () => {
                    casterActor.PlayAnimation(this.AnimationType.ToString(), () =>
                    {
                        onPerformEnd();
                    });
                });
                return;
            }

            

            // Perform animation if specified
            if (AnimationType != ANIMATIONTYPE.NONE)
            {
                casterActor.PlayAnimation(this.AnimationType.ToString(), () =>
                {
                    onPerformEnd();
                });
                return;
            }

            // Perform taunt if specified
            if (SkillType == SKILLTYPE.Taunt)
            {
                //Move closer
                enemyActor.Move((casterActor.transform.position - enemyActor.transform.position).normalized, onPerformEnd);
                return;
            }

            onPerformEnd();
        }

        public void PerformSelf(BaseActorBattler casterActor, Action onPerformEnd)
        {
            //ONLY FOR TARGETINGOPTION.SELF SKILLS
            // Perform teleport if specified
            if (SkillType == SKILLTYPE.Teleport)
            {
                casterActor.PlayAnimation(this.AnimationType.ToString(), () =>
                {
                    //Also play particles and other shit maybe
                    casterActor.GetComponentInChildren<ParticleSystem>().Play(); //to b ereplace
                    casterActor.transform.position = targetPoint;
                    casterActor.GetComponentInChildren<ParticleSystem>().Play();
                    onPerformEnd();
                });
                return;
            }

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
                else
                {
                    onWarmUpEnd();
                    return;
                }
            }

            else if (SkillType == SKILLTYPE.Teleport)
            {
                InputManager.instance.WaitForTargetPoint(point =>
                {
                    this.targetPoint = point;
                    onWarmUpEnd();
                    return;
                });
            }

            else
                onWarmUpEnd();


            //Play an animation before moving,
            //Show skill name,
            //Certain special effects
        }

        public void ApplyDamageEffects(BaseActorBattler casterActor, BaseActorBattler targetActor, BaseReaction targetReaction, Action onDamageEffectsApplied)
        {
            if (Tags.Contains(SKILLTAG.REPEAT_TURN)) BattleManager.GetInstance().RepeatTurn();

            // Apply Damage
            var damage = (this.Damage + casterActor.GetBaseActor().ATK) * targetReaction.damageModifier - targetActor.GetBaseActor().DEF;
            if (damage > 0)
            {
                targetActor.GetBaseActor().DealDamage(damage);
                targetActor.ShowDamagePopup(damage);
                casterActor.PlayAudio("Blow1");
            };
            //targetActor.TriggerHitstop(LinearMap(damage, 0, 20, 0f, 0.5f));

            // Appply Posture
            var postureDamage = PostureDamage * targetReaction.postureModifier;
            //Camera.main.GetComponent<CameraManager>().TriggerShake(LinearMap(postureDamage, 0, 30, 0, 0.015f), LinearMap(postureDamage, 0, 30, 0, 0.5f));
            if (targetActor.GetBaseActor().DealPostureDamage(postureDamage))
            {
                targetActor.TriggerHitstop(0.5f);
                targetActor.PlayAnimation("PostureBroken");
                targetActor.PlayAudio("Attack1");
                UIManager.GetInstance().AddToStoneSlab("Posture break!");
                BattleManager.GetInstance().RepeatTurn(); //Posture break check }
            }
            targetActor.ShowPosturePopup(postureDamage);


            // Apply Knockback
            var direction = (targetActor.transform.position - casterActor.transform.position).normalized;
            if (this.Tags.Contains(SKILLTAG.KNOCKBACK_AIR)) direction = (direction + Vector3.up).normalized;
            targetActor.ApplyKnockback(direction, KnockbackForce * targetReaction.knockbackModifier, () =>
            {
                onDamageEffectsApplied();
            });
        }

        public void PreReaction(BaseActorBattler casterActor, Action onPreReactionEnd)
        { }


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

    public enum ANIMATIONTYPE { NONE, Punch, Kick, Shuriken, Highkick, PalmStrike, Ninjutsu }
    public enum SKILLTYPE { Attack, Projectile, Taunt, Teleport }
    public enum TARGETINGOPTION { SELF, ENEMY }
    public enum SKILLTAG { MOVE_NEAR_ENEMY_BEFORE_ATTACK, PROJECTILE, KNOCKBACK_AIR, MOVE_OFFSET_BEHIND, MOVE_OFFSET_INFRONT, NO_REACTION, REPEAT_TURN }


}