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