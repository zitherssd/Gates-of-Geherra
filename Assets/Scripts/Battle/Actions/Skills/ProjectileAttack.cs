using Assets.Scripts.Actions;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "ProjectileAttack", menuName = "ScriptableObjects/Skills/ProjectileAttack")]
 


    public class ProjectileAttack : BaseSkill
    {
        public GameObject projectilePrefab;
        public float Range;
        public float Damage;
        public float PostureDamage;
        public float KnockbackForce;
        public float SelfForce;
        private Actor caster;

        
        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            caster = casterActor;

            casterActor.state.TransitionTo(casterActor.state.actingState.Set(this, onPerformEnd));
            casterActor.movement.FaceTarget(caster.target.target);
        }
        public override void OnHit()
        {
            caster.movement.AddForce(caster.target.DirectionToClosestEnemy * SelfForce);
            GameObject projectile = Instantiate(projectilePrefab, caster.transform.position + caster.transform.forward * 1f + Vector3.up * 0.5f, caster.transform.rotation, caster.transform);
            projectile.GetComponent<Rigidbody>().AddForce(caster.transform.forward * 3, ForceMode.Impulse);

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