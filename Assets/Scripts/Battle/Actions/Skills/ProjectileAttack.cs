using Assets.Scripts.Actions;
using Assets.Scripts.Battle.States;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "ProjectileAttack", menuName = "ScriptableObjects/Skills/ProjectileAttack")]
 


    public class ProjectileAttack : BaseSkill
    {
        public ANIMATIONTYPE AnimationType;
        public GameObject projectilePrefab;
        public float Range;
        public float Damage;
        public float PostureDamage;
        public float KnockbackForce;
        public float SelfForce;

        
        protected override void PerformSpecific(BaseActorBattler casterActor, Action onPerformEnd)
        {
            var targetActor = GetTarget(casterActor);

            var casterToTarget = (targetActor.transform.position - casterActor.transform.position).normalized;

            if (Tags.Contains(TAG.KILLMOMENTUM)) casterActor.GetComponent<Rigidbody>().velocity = Vector3.zero;

            casterActor.GetComponent<Rigidbody>().AddForce(casterToTarget * 100 * SelfForce);

            casterActor.PlayAnimation(this.AnimationType.ToString(), () => {
                var projectile = Instantiate(projectilePrefab, casterActor.transform.position + casterActor.transform.forward + 0.6f * Vector3.up, Quaternion.identity);
                projectile.GetComponent<Rigidbody>().AddForce((targetActor.transform.position + 0.6f * Vector3.up - projectile.transform.position).normalized * 200f);
                var handler = projectile.GetComponent<ProjectileHandler>();
                handler.Damage = Damage;
                handler.KnockbackForce = KnockbackForce;
                handler.PostureDamage = PostureDamage;
            }, onPerformEnd);
        }

        public void ApplyDamageEffects(BaseActorBattler casterActor, BaseActorBattler targetActor, BaseReaction targetReaction, Action onDamageEffectsApplied)
        {
            // Apply Damage
            var damage = Damage + casterActor.Actor.ATK - targetActor.Actor.DEF;
            if (damage > 0)
            {
                targetActor.ApplyDamage(damage);
            };

            //Apply posture
            if (PostureDamage > 0)
            {
                targetActor.ApplyPosture(PostureDamage);
            }

            // Apply Knockback
            if(KnockbackForce > 0)
            {
            var direction = (targetActor.transform.position - casterActor.transform.position).normalized;
            if (this.Tags.Contains(TAG.KNOCKBACK_BACK)) direction += Vector3.Cross(direction, -Vector3.up);
            if (this.Tags.Contains(TAG.KNOCKBACK_FRONT)) direction += Vector3.Cross(direction, Vector3.up);
            if (this.Tags.Contains(TAG.KNOCKBACK_AIR)) direction = (direction + Vector3.up).normalized;
                targetActor.ApplyKnockback(direction, KnockbackForce);
            }

            // Buildup gain
            casterActor.Actor.currentBuildup += BuildupGain;


            // Wait for 1 frame before exit
            casterActor.StartCoroutine(WaitForOneFrame(() =>
            {
                //if enemy is staggered act again
                if (targetActor.activeStates.OfType<Stagger>().Any())
                {
                    casterActor.Act(onDamageEffectsApplied);
                    return;
                }
                onDamageEffectsApplied();
            }));
        }

        public override bool IsValidAndInRange(BaseActorBattler caster)
        {
            if (!HasUsesLeft()) return false;

            if (IsSkillOnCooldown()) return false;

            var potentialTargets = new List<BaseActorBattler>();

            //Get all active
            if (caster.isControllable())
                potentialTargets = BattleManager.instance.EnemyActors;
            else
                potentialTargets = BattleManager.instance.PlayerActors;
            //If any of them are in range return true
            if (potentialTargets.Any(target => (target.transform.position - caster.transform.position).magnitude < 1 + Range))
                return true;
            else
            {
                return false;
            }
        }
        public BaseActorBattler GetTarget(BaseActorBattler caster)
        {
            if (caster.isControllable())
                return BattleManager.instance.EnemyActors[0];
            else
                return BattleManager.instance.PlayerActors[0];
        }

    }

    public enum PROJECTILETYPE { Shuriken }

}