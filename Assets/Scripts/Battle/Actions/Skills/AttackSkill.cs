using Assets.Scripts.Actions;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "AttackSkill", menuName = "ScriptableObjects/Skills/AttackSkill")]
    public class AttackSkill : BaseSkill
    {
        public float Range;
        public float Damage;
        public float PostureDamage;
        public float KnockbackForce;
        public float windupTimeMult = 1f;
        public float recoveryTimeMult = 1f;
        public STATE state = STATE.uninitialized;
        public float SelfForce;

        private Actor target;
        private Actor casterActor;


        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            //1. Get Target
            //2. Continuously Rotate Incrementally so you are facing target //more work
            //3. Move towards the Direction

            this.casterActor = casterActor;
            casterActor.state.TransitionTo(casterActor.state.actingState.Set(this, onPerformEnd));
            casterActor.movement.AddForce(Direction.normalized * SelfForce);
            //var movetween = LeanTween.move(casterActor.gameObject, casterActor.transform.position + Direction * StickMult, tweenduration).setEase(tweenType);
            target = casterActor.target.ClosestEnemy;
        }
        public void ApplyDamageEffects(Actor casterActor, Actor targetActor, BaseReaction targetReaction, Action onDamageEffectsApplied)
        {
            //Apply posture
            if (PostureDamage > 0)
            {
                targetActor.ApplyPosture(PostureDamage);
            }

            // Apply Damage
            var damage = Damage + casterActor.ActorData.ATK - targetActor.ActorData.DEF;
            if (damage > 0)
            {
                var hitstop = StaticHelpers.LinearMap(damage, 0.2f, 15, 0.083f, 0.420f);
                BattleManager.instance.ApplyHitstop(hitstop);
                targetActor.ApplyDamage(damage);
            };

            // Apply Knockback
            if (KnockbackForce > 0)
            {
                var direction = (targetActor.transform.position - casterActor.transform.position).normalized;
                if (this.Tags.Contains(TAG.KNOCKBACK_BACK))
                {
                    Vector3 cameraForward = Camera.main.transform.forward;
                    Vector3 aux = Vector3.Cross(direction, -Vector3.up);

                    if (Vector3.Dot(aux, cameraForward) < 0f) //if its oppsoite the camera
                    {
                        aux = -aux; //make it face the camera
                    }
                    direction += aux;
                }

                if (this.Tags.Contains(TAG.KNOCKBACK_FRONT))
                {
                    Vector3 cameraForward = Camera.main.transform.forward;
                    Vector3 aux = Vector3.Cross(direction, Vector3.up);

                    if (Vector3.Dot(aux, cameraForward) > 0f) //if it's the same as the camera
                    {
                        aux = -aux; //make it opposite
                    }
                    direction += aux;
                }


                if (this.Tags.Contains(TAG.KNOCKBACK_AIR)) { direction = (direction + Vector3.up).normalized; }
                targetActor.ApplyKnockback(direction, KnockbackForce);
            }



            onDamageEffectsApplied?.Invoke();
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


        public override void OnHit()
        {
            RaycastHit hit;
            var targetDir = target.transform.position - casterActor.transform.position;
            Debug.DrawRay(casterActor.transform.position + Vector3.up * 0.6f, targetDir * Range, Color.green, 1f, false);
            if (Physics.SphereCast(casterActor.transform.position + Vector3.up * 0.6f, 0.1f, targetDir, out hit, Range))
            {
                ApplyDamageEffects(casterActor, target, BaseReaction.NoReaction, null);
                return;
            }
            else
            {
                Debug.Log($"{casterActor.ActorData.name} missed performing {this.Name}!");
            }
        }

        public override void OnEnterWindup(Animator animator)
        {
            CalculateDuration(animator);
            animator.speed = windupTimeMult;
            state = STATE.windup;
        }

        public override void OnEnterRecovery(Animator animator)
        {
            animator.speed = recoveryTimeMult;
            state = STATE.recovery;
        }

        public void OnReaction(Animator animator)
        {

        }

        public enum STATE { uninitialized, windup, recovery };
    }
}