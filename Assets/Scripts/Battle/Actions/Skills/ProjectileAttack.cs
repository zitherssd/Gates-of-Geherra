using Assets.Scripts.Actions;
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
        public ANIMATION Animation;
        public GameObject projectilePrefab;
        public float Range;
        public float Damage;
        public float PostureDamage;
        public float KnockbackForce;
        public float SelfForce;

        
        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            var targetActor = casterActor.target.ClosestEnemy;

            var casterToTarget = (targetActor.transform.position - casterActor.transform.position).normalized;


            casterActor.GetComponent<Rigidbody>().AddForce(casterToTarget * 100 * SelfForce);

            casterActor.PlayAnimation(this.Animation.ToString(), () => {
                var projectile = Instantiate(projectilePrefab, casterActor.transform.position + casterActor.transform.forward + 0.6f * Vector3.up, Quaternion.identity);
                projectile.GetComponent<Rigidbody>().AddForce((targetActor.transform.position + 0.6f * Vector3.up - projectile.transform.position).normalized * 200f);
                var handler = projectile.GetComponent<ProjectileHandler>();
                handler.Damage = Damage;
                handler.KnockbackForce = KnockbackForce;
                handler.PostureDamage = PostureDamage;
            }, onPerformEnd);
        }

        public override bool IsValidAndInRange(Actor caster)
        {
            if (!HasUsesLeft()) return false;

            if (IsSkillOnCooldown()) return false;

            var potentialTargets = new List<Actor>();

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
    }

    public enum PROJECTILETYPE { Shuriken }
    public enum ANIMATION { NONE, Punch, Kick, Shuriken, Highkick, PalmStrike, Ninjutsu, ForwardPunch, ThrowStar }


}