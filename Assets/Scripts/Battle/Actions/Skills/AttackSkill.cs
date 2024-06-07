using Assets.Scripts.Actions;
using Assets.Scripts.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            var targetActor = GetTarget(casterActor);
            var targetActorPositionInOneSecond = targetActor.rigidbody.position + targetActor.rigidbody.velocity * Delay;
            
            var casterVelocity = casterActor.rigidbody.velocity;
            var casterActorPositionInOneSecond = casterActor.transform.position + casterVelocity * Delay;

            var casterToTarget = (targetActor.transform.position - casterActor.transform.position).normalized;
            var casterToTargetInOneSecond = (targetActorPositionInOneSecond - casterActor.transform.position).normalized;

            var targetDirection = casterToTargetInOneSecond;
            Debug.DrawRay(casterActor.transform.position + Vector3.up * 0.6f, targetDirection * Range, Color.green, 3f, false);
            Debug.DrawRay(casterActor.transform.position + Vector3.up * 0.1f, targetDirection * Range, Color.green, 3f, false);
            Debug.DrawRay(casterActor.transform.position + Vector3.up * 0.01f, targetDirection * Range, Color.green, 3f, false);

            var desiredMoveDirection = GetRelativeToCamera(StickValue);
            if (casterActor.isControllable())
                casterActor.GetComponent<Rigidbody>().AddForce(desiredMoveDirection * 100 * SelfForce);
            else casterActor.GetComponent<Rigidbody>().AddForce(new Vector3(StickValue.x,0,StickValue.y) * 100 * SelfForce);

            casterActor.PlayAnimation(Animation.ToString(), () => {

                Debug.DrawRay(casterActor.transform.position + Vector3.up * 0.6f, targetDirection * Range, Color.green, 2f, false);

                RaycastHit hit;

                if (Physics.SphereCast(casterActor.transform.position + Vector3.up * 0.6f, 0.1f, targetDirection, out hit, Range))
                {
                    ApplyDamageEffects(casterActor, targetActor, BaseReaction.NoReaction, onPerformEnd);
                    return;
                }
                else
                {
                    Debug.Log($"{casterActor.ActorData.name} missed performing {this.Name}!");
                }
            }, () => {
                if (targetActor.state.CurrentState == targetActor.state.staggerState) return;
                var validReactions = targetActor.GetValidReactionsForSkill();
                if (targetActor.isControllable())
                {
                    Time.timeScale = 0f; UIManager.GetInstance().ShowUI();
                    var reactionsToDraw = new List<BaseAction>();
                    reactionsToDraw.AddRange(validReactions);
                    UIManager.GetInstance().DrawActionsAndWaitForSelectionOrNull(reactionsToDraw, selectedReaction =>
                    {
                        Time.timeScale = 1f; UIManager.GetInstance().HideUI();
                        if (selectedReaction != null && selectedReaction != BaseReaction.NoReaction)
                        {
                            //if (selectedReaction.Tags.Contains(TAG.USESLIDER))
                                //selectedReaction.SliderValue = InputManager.instance.SliderValue;
                            //if (selectedReaction.Tags.Contains(TAG.USEKNOB))
                                //selectedReaction.StickValue = InputManager.instance.StickValue;
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
        public void ApplyDamageEffects(Actor casterActor, Actor targetActor, BaseReaction targetReaction, Action onDamageEffectsApplied)
        {
            // Apply Damage
            var damage = Damage + casterActor.ActorData.ATK - targetActor.ActorData.DEF;
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
                if (targetActor.state.CurrentState == targetActor.state.staggerState)
                {
                    casterActor.KillAnimationEndEvent();
                    casterActor.Act(onDamageEffectsApplied);
                    return;
                }
                //onDamageEffectsApplied();
            }));
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
        public Actor GetTarget(Actor caster)
        {
            if (caster.isControllable())
                    return BattleManager.instance.EnemyActors[0];
            else
                    return BattleManager.instance.PlayerActors[0];
        }

        public enum ANIMATION { NONE, Punch, Kick, Shuriken, Highkick, PalmStrike, Ninjutsu, ForwardPunch, ThrowStar }
    }
}