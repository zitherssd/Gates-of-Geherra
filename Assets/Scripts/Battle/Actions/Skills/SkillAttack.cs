using Assets.Scripts.Actions;
using Assets.Scripts.Battle.States;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "AttackSkill", menuName = "ScriptableObjects/Skills/AttackSkill")]
    public class SkillAttack : BaseSkill
    {
        public ANIMATIONTYPE AnimationType;
        public float Range;
        public float Damage;
        public float PostureDamage;
        public float KnockbackForce;
        public float SelfForce;

        protected override void PerformSpecific(BaseActorBattler casterActor, Action onPerformEnd)
        {
            var targetActor = GetTarget(casterActor);

            var casterToTarget = (targetActor.transform.position - casterActor.transform.position).normalized;
            casterToTarget.y = 0;

            if (Tags.Contains(TAG.KILLMOMENTUM)) casterActor.GetComponent<Rigidbody>().velocity = Vector3.zero;

            casterActor.GetComponent<Rigidbody>().AddForce(casterToTarget * 100 * SelfForce * SliderValue);

            casterActor.PlayAnimation(this.AnimationType.ToString(), () => {

                Debug.DrawRay(casterActor.transform.position + Vector3.up * 0.6f, casterToTarget * Range, Color.green, 0.3f);

                RaycastHit hit;

                if (Physics.SphereCast(casterActor.transform.position + Vector3.up * 0.6f, 0.1f, casterToTarget, out hit, Range))
                {
                    ApplyDamageEffects(casterActor, targetActor, BaseReaction.NoReaction, onPerformEnd);
                    return;
                }
                else
                {
                    Debug.Log($"{casterActor.Actor.name} missed performing {this.Name}!");
                }
            }, () => {
                if (targetActor.activeStates.OfType<Stagger>().Any()) return;
                var validReactions = targetActor.GetValidReactionsForSkill();
                if (targetActor.isControllable())
                {
                    Time.timeScale = 0f;
                    var reactionsToDraw = new List<BaseAction>();
                    reactionsToDraw.AddRange(validReactions);
                    UIManager.GetInstance().DrawActionsAndWaitForSelectionOrNull(reactionsToDraw, selectedReaction =>
                    {
                        Time.timeScale = 1f;
                        if (selectedReaction != null && selectedReaction != BaseReaction.NoReaction)
                        {
                            if (selectedReaction.Tags.Contains(TAG.USESLIDER))
                                selectedReaction.SliderValue = InputManager.instance.SliderValue;
                            if (selectedReaction.Tags.Contains(TAG.USEKNOB))
                                selectedReaction.StickValue = InputManager.instance.StickValue;
                            selectedReaction.Perform(targetActor, null);
                        }
                    });
                }
                else
                {
                    if(validReactions.Count > 0)
                    {
                        var selectedReaction = validReactions[RandomFromList(validReactions.Count)];
                        selectedReaction.Perform(targetActor, null);
                    }
                }
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

            // Apply Knockback
            if (KnockbackForce > 0)
            {
                var direction = (targetActor.transform.position - casterActor.transform.position).normalized;
                if (this.Tags.Contains(TAG.KNOCKBACK_BACK)) direction += Vector3.Cross(direction, -Vector3.up);
                if (this.Tags.Contains(TAG.KNOCKBACK_FRONT)) direction += Vector3.Cross(direction, Vector3.up);
                if (this.Tags.Contains(TAG.KNOCKBACK_AIR)) { direction = (direction + Vector3.up).normalized; targetActor.activeStates.Add(new Midair(targetActor)); }
                targetActor.ApplyKnockback(direction, KnockbackForce);
            }

            //Apply posture
            if (PostureDamage > 0)
            {
                targetActor.ApplyPosture(PostureDamage);
            }

            casterActor.Actor.currentBuildup += BuildupGain;

            // Wait for 1 frame before exit
            casterActor.StartCoroutine(WaitForOneFrame(() =>
            {
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
}