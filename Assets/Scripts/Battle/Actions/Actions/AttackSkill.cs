using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "AttackSkill", menuName = "ScriptableObjects/Action/AttackAction")]
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
        public List<Vector3> HitboxPoints;
        public float HitboxScale = 1f;
        private Actor.Actor target;
        private Actor.Actor casterActor;
        public float windupPostureDamageMult = 1f;
        public float recoveryPostureDamageMult = 1f;
        public float BuildupGainOnHit;

        protected override void PerformSpecific(Actor.Actor casterActor, Action onPerformEnd)
        {
            //1. Get Target
            //2. Continuously Rotate Incrementally so you are facing target //more work
            //3. Move towards the Direction
            OnCancel = onPerformEnd;
            this.casterActor = casterActor;
            var actingState = casterActor.state.TransitionTo<ActingState>();
            
            actingState.Set(this, onPerformEnd);
            casterActor.state.TransitionTo<ActingState>().Set(this, onPerformEnd);
            if (Type == BUTTONTYPE.VECTOR)
            {
                casterActor.movement.AddForce(Direction.normalized * SelfForce);
                casterActor.movement.FaceDirection(Direction.normalized);
            }

            else
            {
                casterActor.movement.AddForce(casterActor.target.DirectionToClosestEnemy * SelfForce);
                casterActor.movement.FaceDirection(casterActor.target.DirectionToClosestEnemy);
            }
            if (Tags.Contains(TAG.FACECLOSEST)) casterActor.movement.FaceDirection(casterActor.target.DirectionToClosestEnemy);

            //var movetween = LeanTween.move(casterActor.gameObject, casterActor.transform.position + Direction * StickMult, tweenduration).setEase(tweenType);
            target = casterActor.target.ClosestEnemy;
            casterActor.PostureRecieved += ApplyPostureModifier;
        }

        public void Unsubscribe()
        {
            casterActor.PostureRecieved -= ApplyPostureModifier;
        }
        public void ApplyDamageEffects(Actor.Actor casterActor, Actor.Actor targetActor, Action onDamageEffectsApplied)
        {
            var targetBlocking = targetActor.state.IsBlocking();
            if (casterActor.isControllable && !targetBlocking)
                UIManager.instance.GainMeter(SlowdownMeterGain);
            if (targetBlocking)
                casterActor.ActorData.ChangeBuildup(BuildupGainOnHit / 2);
            else
                casterActor.ActorData.ChangeBuildup(BuildupGainOnHit);

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
                targetActor.ApplyDamage(damage);
            };

            // Apply Knockback
            if (KnockbackForce > 0)
            {
                Vector3 direction;
                if (Type is BUTTONTYPE.VECTOR)
                    direction = Direction.normalized;
                else
                    direction = (targetActor.transform.position - casterActor.transform.position).normalized;
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
        public override bool IsValidAndInRange(Actor.Actor caster)
        {
            string invalidReason;
            if (IsValid(caster, out invalidReason))
            {
                if (caster.target.DistanceToClosestEnemy < HitboxPoints.Max(point => point.z) + SelfForce / 2)
                    return true;
                else
                    return false;
            }
            Debug.Log(invalidReason);
            return false;
        }


        public override void OnHit()
        {

            //RaycastHit hit;
            //var targetDir = target.transform.position - casterActor.transform.position;
            casterActor.effects.ClearHitbox();


            var hits = CheckEnemiesInsideHitbox();
            foreach (var hit in hits)
            {
                if (Tags.Contains(TAG.TECH) && !target.state.IsBlocking())
                    ApplyDamageEffects(casterActor, hit, OnCancel);
                else
                    ApplyDamageEffects(casterActor, hit, null);
            }
        }

        public override void OnEnterWindup(Animator animator)
        {
            casterActor.effects.SetHitbox(HitboxPoints.Select(point => point * HitboxScale).ToList());
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

        private List<Actor.Actor> CheckEnemiesInsideHitbox()
        {
            // Transform hitbox points to world space based on caster's position and orientation
            List<Vector3> transformedPoints = new List<Vector3>();
            var validActors = new List<Actor.Actor>();

            foreach (var point in HitboxPoints)
            {
                // Rotate and position each point relative to the caster
                Vector3 worldPoint = casterActor.transform.position + casterActor.transform.TransformDirection(point * HitboxScale);
                transformedPoints.Add(worldPoint);
            }

            List<Actor.Actor> enemyActors = casterActor.isControllable
                ? BattleManager.instance.EnemyActors
                : BattleManager.instance.PlayerActors;


            foreach (var enemy in enemyActors.Where(actor => actor.state.IsAlive()))
            {
                Vector3 enemyPosition = enemy.transform.position;


                // Check if the center of the capsule is inside
                if (Intersections.IsPointInsidePolygon(enemyPosition, transformedPoints))
                {
                    validActors.Add(enemy);
                    continue;
                }
                float capsuleRadius = enemy.GetComponent<CapsuleCollider>().radius;

                // Check if the capsule collider intersects the polygon
                foreach (var edgeStart in transformedPoints)
                {
                    int nextIndex = (transformedPoints.IndexOf(edgeStart) + 1) % transformedPoints.Count;
                    Vector3 edgeEnd = transformedPoints[nextIndex];

                    if (Intersections.IsCircleIntersectingLine(enemyPosition, capsuleRadius, edgeStart, edgeEnd))
                    {
                        validActors.Add(enemy);
                        break;
                    }
                }
            }

            return validActors;
        }


        internal bool IsWithinRange(Actor.Actor actor, Actor.Actor target)
        {
            throw new NotImplementedException();
        }

        public float ApplyPostureModifier(float posture)
        {
            if (state == STATE.windup)
                return posture * windupPostureDamageMult;
            if (state == STATE.recovery)
                return posture * recoveryPostureDamageMult;
            return posture;
        }
    }
}