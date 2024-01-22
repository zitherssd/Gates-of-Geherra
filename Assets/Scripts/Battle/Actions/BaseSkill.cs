using Assets.Scripts.Battle.States;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Actions
{
    [CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skill", order = 1)]
    public class BaseSkill : BaseAction
    {

        public ANIMATIONTYPE AnimationType;
        public SKILLTYPE SkillType;
        public TARGETINGOPTION TargetOption;
        public float Range;
        public float Damage;
        public float PostureDamage;
        public float KnockbackForce;
        public float SelfForce;
        private Vector3 targetPoint;

        public override void Perform(BaseActorBattler casterActor, Action onPerformEnd)
        {
            if (casterActor.isControllable())
            {
                if (TargetOption == TARGETINGOPTION.ENEMY)
                    InputManager.instance.WaitForTargetActor(targetActor => PerformTarget(casterActor, targetActor, null, onPerformEnd));
                if (TargetOption == TARGETINGOPTION.SELF)
                    PerformSelf(casterActor, onPerformEnd);
                return;
            }
            else
            {
                var targetActor = BattleManager.GetInstance().PlayerActors[0];
                if (TargetOption == TARGETINGOPTION.ENEMY)
                    PerformTarget(casterActor, targetActor, null, onPerformEnd);
                if (TargetOption == TARGETINGOPTION.SELF)
                    PerformSelf(casterActor, onPerformEnd);
                return;
            }
        }
        public void PerformTarget(BaseActorBattler casterActor, BaseActorBattler targetActor, BaseReaction reaction, Action onPerformEnd)
        {
            var casterToTarget = (targetActor.transform.position - casterActor.transform.position).normalized;
            
            casterActor.rigidbody.AddForce(casterToTarget * 100 * SelfForce * SliderValue);

            //if (Tags.Contains(TAG.APPLYROOTMOTION)) casterActor.SetRootMotion(true);

            casterActor.PlayAnimation(this.AnimationType.ToString(), () =>
            {
                Debug.DrawRay(casterActor.transform.position + Vector3.up * 0.6f, casterToTarget * Range, Color.green, 0.3f);

                //casterActor.SetRootMotion(false);
                //Physics.IgnoreLayerCollision(casterActor.gameObject.layer, casterActor.gameObject.layer, true);
                RaycastHit hit;
                if (Physics.SphereCast(casterActor.transform.position + Vector3.up * 0.6f, 0.1f, casterToTarget, out hit, Range))
                {
                    Debug.Log("Hit something with sphere cast: " + hit.collider.gameObject.name);
                    ApplyDamageEffects(casterActor, targetActor, BaseReaction.NoReaction, onPerformEnd);
                    return;
                }
                else
                {
                    Debug.Log("Didn't Hit Something with sphere cast");
                    onPerformEnd();
                }
            }, () =>
            {
                var validReactions = targetActor.GetValidReactionsForSkill();
                if (targetActor.isControllable())
                {
                    Time.timeScale = 0f;
                    UIManager.GetInstance().DrawReactionsAndWaitForSelectionOrNull(validReactions, selectedReaction =>
                    {
                        Time.timeScale = 1f;
                        if(selectedReaction != null && selectedReaction != BaseReaction.NoReaction)
                        {
                            if (selectedReaction.Tags.Contains(TAG.USESLIDER))
                                selectedReaction.SliderValue = InputManager.instance.SliderValue;
                            if (selectedReaction.Tags.Contains(TAG.USEKNOB))
                                selectedReaction.StickValue = InputManager.instance.StickValue;
                        }
                        selectedReaction.React(this, targetActor, null);
                    });
                }
                else
                {
                    var selectedReaction = validReactions[RandomFromList(validReactions.Count)];
                    selectedReaction.React(this, targetActor, null);
                }
            });
        }

        public float GetSliderValue(BaseActorBattler casterActor)
        {
            if (casterActor.isControllable())
            {
                return InputManager.instance.SliderValue;
            }
            else
                return 1;
        }

        public Vector2 GetStickValue(BaseActorBattler casterActor)
        {
            if (casterActor.isControllable())
            {
                return InputManager.instance.StickValue;
            }
            else
                return Vector2.zero;
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
                    casterActor.transform.position = casterActor.transform.position + GetRelativeToCamera(StickValue) * 10f;
                    casterActor.GetComponentInChildren<ParticleSystem>().Play();
                    onPerformEnd();
                }, null);
                return;
            }

            onPerformEnd();
        }

        public void WarmUp(BaseActorBattler casterActor, BaseActorBattler targetActor, Action onWarmUpEnd)
        {
            if (this.Tags.Contains(TAG.MOVE_NEAR_ENEMY_BEFORE_ATTACK))
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

            //else if (SkillType == SKILLTYPE.Teleport)
            //{
            //    InputManager.instance.WaitForTargetPoint(point =>
            //    {
            //        this.targetPoint = point;
            //        onWarmUpEnd();
            //        return;
            //    });
            //}

            else
                onWarmUpEnd();


            //Play an animation before moving,
            //Show skill name,
            //Certain special effects
        }

        public void ApplyDamageEffects(BaseActorBattler casterActor, BaseActorBattler targetActor, BaseReaction targetReaction, Action onDamageEffectsApplied)
        {
            // Apply Damage
            var damage = (this.Damage + casterActor.GetBaseActor().ATK) * targetReaction.damageModifier - targetActor.GetBaseActor().DEF;
            if (damage > 0)
            {
                targetActor.onDamageRecieved.Invoke(damage);
            };

            //Apply posture
            if (PostureDamage > 0)
            {
                if (targetActor.Actor.currentPosture - PostureDamage <= targetActor.Actor.maxPosture / 2)
                {
                }

                targetActor.onPostureRecieved.Invoke(PostureDamage);
            }

            // Apply Knockback
            var direction = (targetActor.transform.position - casterActor.transform.position).normalized;
            if (this.Tags.Contains(TAG.KNOCKBACK_BACK)) direction = Vector3.Cross(direction, Vector3.up);
            if (this.Tags.Contains(TAG.KNOCKBACK_FRONT)) direction += Vector3.Cross(direction, -Vector3.up);
            if (this.Tags.Contains(TAG.KNOCKBACK_AIR)) direction = (direction + Vector3.up).normalized;
            targetActor.onKnockbackRecieved.Invoke(direction, KnockbackForce);
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

            //
        }

        public void PreReaction(BaseActorBattler casterActor, Action onPreReactionEnd)
        { }

        private System.Collections.IEnumerator WaitForOneFrame(Action action)
        {
            // This will wait for one frame
            yield return null;

            // Code here will be executed on the frame after the wait
            action.Invoke();
        }

        public override bool IsValid(BaseActorBattler caster, out string InvalidReason)
        {
            InvalidReason = "";
            if (!HasUsesLeft()) return false;

            if (IsSkillOnCooldown())
            {
                InvalidReason = $"usable in {currentCooldownTurns}";
                return false;
            }

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

        public override bool IsValid()
        {
            if (!HasUsesLeft()) return false;

            if (IsSkillOnCooldown()) return false;

            return true;
        }


        public override bool IsValidAndInRange(BaseActorBattler caster)
        {
            if (!HasUsesLeft()) return false;

            if (IsSkillOnCooldown()) return false;

            var potentialTargets = new List<BaseActorBattler>();

            //Get all active
            if (caster.isControllable())
                potentialTargets = BattleManager.GetInstance().EnemyActors;
            else
                potentialTargets = BattleManager.GetInstance().PlayerActors;
            //If any of them are in range return true
            if (potentialTargets.Any(target => (target.transform.position - caster.transform.position).magnitude < Range + 1))
                return true;
            else
            {
                return false;
            }
        }
        private int RandomFromList(int count)
        {
            var random = new System.Random();
            var index = random.Next(count);
            return index;
        }

        private Vector3 GetRelativeToCamera(Vector2 direction)
        {
            var camera = Camera.main;
            var forward = camera.transform.forward; forward.y = 0;
            var right = camera.transform.right; right.y = 0;
            forward.Normalize(); right.Normalize();

            var desiredMoveDirection = forward * direction.y + right * direction.x;
            return desiredMoveDirection;
        }
    }

    public enum ANIMATIONTYPE { NONE, Punch, Kick, Shuriken, Highkick, PalmStrike, Ninjutsu, ForwardPunch }
    public enum SKILLTYPE { Attack, Projectile, Taunt, Teleport }
    public enum TARGETINGOPTION { SELF, ENEMY }
}