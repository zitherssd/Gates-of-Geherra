using Assets.Scripts.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "AttackSkill", menuName = "ScriptableObjects/Skills/AttackSkill")]
    public class AttackSkill : BaseSkill
    {
        public ANIMATION Animation;
        public float Range;
        public float Damage;
        public float PostureDamage;
        public float KnockbackForce;
        public float SelfForce;
        public float Delay;

        public bool preciseattack = true;
        private bool hit = false;

        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            hit = false;
            var targetActor = casterActor.target.ClosestEnemy;
            var targetActorPositionInOneSecond = targetActor.rigidbody.position + targetActor.rigidbody.velocity * Delay;

            var casterVelocity = casterActor.rigidbody.velocity;
            var casterActorPositionInOneSecond = casterActor.transform.position + casterVelocity * Delay;

            var casterToTarget = (targetActor.transform.position - casterActor.transform.position).normalized;
            var casterToTargetInOneSecond = (targetActorPositionInOneSecond - casterActor.transform.position).normalized;

            var targetDirection = casterToTargetInOneSecond;
            Debug.DrawRay(casterActor.transform.position + Vector3.up * 0.5f, targetDirection * Range, Color.red, 3f, false);
            Debug.DrawRay(casterActor.transform.position + Vector3.up * 0.1f, targetDirection * Range, Color.red, 3f, false);

            //Apply selfForce
            if (!Tags.Contains(TAG.USESTICK))
                casterActor.GetComponent<Rigidbody>().AddForce(100 * SelfForce * casterActor.target.DirectionToClosestEnemy);
            else
                casterActor.GetComponent<Rigidbody>().AddForce(100 * SelfForce * new Vector3(Direction.x, 0, Direction.y));

            casterActor.PlayAnimation(Animation.ToString(), () =>
            {
                if (this.hit) return;
                RaycastHit hit;
                if (!preciseattack)
                {
                    Debug.DrawRay(casterActor.transform.position + Vector3.up * 0.6f, targetDirection * Range, Color.green, 1f, false);
                    if (Physics.SphereCast(casterActor.transform.position + Vector3.up * 0.6f, 0.1f, targetDirection, out hit, Range))
                    {
                        ApplyDamageEffects(casterActor, targetActor, BaseReaction.NoReaction, onPerformEnd);
                        return;
                    }
                    else
                    {
                        Debug.Log($"{casterActor.ActorData.name} missed performing {this.Name}!");
                    }
                }
                else
                {
                    Debug.DrawRay(casterActor.transform.position + Vector3.up * 0.6f, (targetActor.transform.position - casterActor.transform.position) * Range, Color.red, 1f, false);
                    if (Physics.SphereCast(casterActor.transform.position + Vector3.up * 0.6f, 0.1f, targetActor.transform.position - casterActor.transform.position, out hit, Range))
                    {
                        ApplyDamageEffects(casterActor, targetActor, BaseReaction.NoReaction, onPerformEnd);
                        return;
                    }
                    else
                    {
                        Debug.Log($"{casterActor.ActorData.name} missed performing {this.Name}!");
                    }
                }
            }, () => //OnReaction
            {
                if (targetActor.state.CurrentState == targetActor.state.staggerState || targetActor.state.CurrentState == targetActor.state.airStaggerState) return;

                targetActor.ai.ChooseReaction(chosenReaction =>
                {
                    targetActor.UseAction(chosenReaction, null);
                    chosenReaction.Perform(targetActor,null);
                }, this);
            }, onPerformEnd);
        }

        public void OnHit()
        {

        }


        public void ApplyDamageEffects(Actor casterActor, Actor targetActor, BaseReaction targetReaction, Action onDamageEffectsApplied)
        {
            Debug.Log(casterActor.rigidbody.velocity.magnitude);
            this.hit = true;
            // Apply Damage
            var damage = Damage + casterActor.ActorData.ATK - targetActor.ActorData.DEF;
            if (damage > 0)
            {
                var hitstop = LinearMap(damage, 0.2f, 15, 0.083f, 0.420f);
                BattleManager.instance.ApplyHitstop(hitstop);
                targetActor.ApplyDamage(damage);
            };

            // Apply Knockback
            if (KnockbackForce > 0)
            {
                var direction = (targetActor.transform.position - casterActor.transform.position).normalized;
                if (this.Tags.Contains(TAG.KNOCKBACK_BACK)) direction += Vector3.Cross(direction, -Vector3.up);
                if (this.Tags.Contains(TAG.KNOCKBACK_FRONT)) direction += Vector3.Cross(direction, Vector3.up);
                if (this.Tags.Contains(TAG.KNOCKBACK_AIR)) { direction = (direction + Vector3.up).normalized; }
                targetActor.ApplyKnockback(direction, KnockbackForce);
            }

            //Apply posture
            if (PostureDamage > 0)
            {
                targetActor.ApplyPosture(PostureDamage);
            }

            casterActor.ActorData.currentBuildup += BuildupGain;

            // Wait for 1 frame before exit
            casterActor.StartCoroutine(WaitForOneFrame(() =>
            {
                if (targetActor.state.CurrentState == targetActor.state.staggerState || targetActor.state.CurrentState == targetActor.state.airStaggerState)
                {
                    casterActor.Act(onDamageEffectsApplied);
                    return;
                }
                //onDamageEffectsApplied();
            }));
        }
        float LinearMap(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((input - inputMin) / (inputMax - inputMin));
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
        public enum ANIMATION { NONE, Punch, Kick, Shuriken, Highkick, PalmStrike, Ninjutsu, ForwardPunch, ThrowStar, ForwardKick }
    }
}