using Assets.Scripts.Battle.Actions.Actions.Effects;
using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "ProjectileAttack", menuName = "ScriptableObjects/Action/ProjectileAttack")]
 


    public class ProjectileAttack : BaseSkill
    {
        public GameObject projectilePrefab;
        public float Range;
        public float Damage;
        public float PostureDamage;
        public float KnockbackForce;
        public float SelfForce;
        public float ThrowSpeed = 3f;
        public float SizeMultiplier = 1f;
        [SerializeReference, SubclassSelector]
        public List<IProjectileEffect> OnProjectileHitEffects = new List<IProjectileEffect>();
        [SerializeReference, SubclassSelector]
        public List<IEffect> OnHitEffects = new List<IEffect>();
        public float windupTimeMult = 1f;
        public float recoveryTimeMult = 1f;
        private Actor.Actor caster;
        private Action _onPerformEnd;

        
        protected override void PerformSpecific(Actor.Actor casterActor, Action onPerformEnd)
        {
            caster = casterActor;
            _onPerformEnd = onPerformEnd;
            casterActor.state.TransitionTo<ActingState>().Set(this, onPerformEnd);
            casterActor.movement.FaceDirection(caster.target.DirectionToClosestEnemy);
        }
        public override void OnHit()
        {
            caster.movement.AddForce(caster.target.DirectionToClosestEnemy * SelfForce);
            GameObject projectile = Instantiate(projectilePrefab, caster.transform.position + caster.target.DirectionToClosestEnemy * 1f + Vector3.up * 0.5f, caster.transform.rotation);
            projectile.GetComponent<ProjectileHandler>().Initialize(caster, this);
            
            if(Type == BUTTONTYPE.VECTOR)
            {
                projectile.GetComponent<Rigidbody>().AddForce(Direction * ThrowSpeed, ForceMode.Impulse);
            }
            else
                projectile.GetComponent<Rigidbody>().AddForce(caster.transform.forward * ThrowSpeed, ForceMode.Impulse);
            if(Tags.Contains(TAG.TECH))
            {
                _onPerformEnd.Invoke();
            }
        }

        public override bool IsValidAndInRange(Actor.Actor caster)
        {
            if (!HasUsesLeft()) return false;

            if (IsSkillOnCooldown()) return false;

            return true;
        }

        public override void OnEnterWindup(Animator animator)
        {
            animator.speed = windupTimeMult;
        }
        public override void OnEnterRecovery(Animator animator)
        {
            animator.speed = recoveryTimeMult;
        }
    }

    public enum PROJECTILETYPE { Shuriken }


}