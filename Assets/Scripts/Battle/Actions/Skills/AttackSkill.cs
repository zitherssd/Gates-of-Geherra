using Assets.Scripts.Actions;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
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
        public List<Vector3> HitboxPoints;
        private Actor target;
        private Actor casterActor;
        public float windupPostureDamageMult = 1f;
        public float recoveryPostureDamageMult = 1f;

        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            //1. Get Target
            //2. Continuously Rotate Incrementally so you are facing target //more work
            //3. Move towards the Direction
            cancel = onPerformEnd;
            this.casterActor = casterActor;
            casterActor.state.TransitionTo(casterActor.state.actingState.Set(this, onPerformEnd));
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
        public void ApplyDamageEffects(Actor casterActor, Actor targetActor, BaseReaction targetReaction, Action onDamageEffectsApplied)
        {
            // Apply Knockback
            if (KnockbackForce > 0)
            {
                Vector3 direction;
                if (Tags.Contains(TAG.USESTICK))
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

            //Apply posture
            if (PostureDamage > 0)
            {
                targetActor.ApplyPosture(PostureDamage);
                if (casterActor.isControllable() && targetActor.state.CurrentState != targetActor.state.blockState)
                {
                    UIManager.instance.GainMeter(SlowdownMeterGain);
                }
            }

            // Apply Damage
            var damage = Damage + casterActor.ActorData.ATK - targetActor.ActorData.DEF;
            if (damage > 0)
            {
                var hitstop = StaticHelpers.LinearMap(damage, 0.2f, 15, 0.083f, 0.420f);
                targetActor.ApplyDamage(damage);
            };

   


            onDamageEffectsApplied?.Invoke();
        }
        public override bool IsValidAndInRange(Actor caster)
        {
            if (IsValid(caster, out _))
            {
                if (caster.target.DistanceToClosestEnemy < Range + SelfForce)
                    return true;
                else
                    return false;
            }
            return false;
        }


        public override void OnHit()
        {

            //RaycastHit hit;
            //var targetDir = target.transform.position - casterActor.transform.position;
            casterActor.effects.Clear();


            var hits = CheckEnemiesInsideHitbox();
            foreach(var hit in hits)
            {
                if (Tags.Contains(TAG.TECH) && target.state.CurrentState != target.state.blockState)
                    ApplyDamageEffects(casterActor, hit, BaseReaction.NoReaction, cancel);
                else
                    ApplyDamageEffects(casterActor, hit, BaseReaction.NoReaction, null);
            }
            //Debug.DrawRay(casterActor.transform.position + Vector3.up * 0.6f, targetDir * Range, Color.green, 1f, false);
            //if (Physics.SphereCast(casterActor.transform.position + Vector3.up * 0.6f, 0.1f, targetDir, out hit, Range))
            //{
            //    if (Tags.Contains(TAG.TECH))
            //        ApplyDamageEffects(casterActor, target, BaseReaction.NoReaction, cancel);
            //    else
            //        ApplyDamageEffects(casterActor, target, BaseReaction.NoReaction, null);
            //    return;
            //}
            //else
            //{
            //    Debug.Log($"{casterActor.ActorData.name} missed performing {this.Name}!");
            //}
        }

        public override void OnEnterWindup(Animator animator)
        {
            //here it should set the target for the drawer so it can draw it 
            casterActor.effects.SetHitbox(HitboxPoints);
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

        private List<Actor> CheckEnemiesInsideHitbox()
        {
            // Transform hitbox points to world space based on caster's position and orientation
            List<Vector3> transformedPoints = new List<Vector3>();
            var validActors = new List<Actor>();

            foreach (var point in HitboxPoints)
            {
                // Rotate and position each point relative to the caster
                Vector3 worldPoint = casterActor.transform.position + casterActor.transform.TransformDirection(point);
                transformedPoints.Add(worldPoint);
            }

            List<Actor> enemyActors = casterActor.isControllable()
                ? BattleManager.instance.EnemyActors
                : BattleManager.instance.PlayerActors;


            foreach (var enemy in enemyActors)
            {
                Vector3 enemyPosition = enemy.transform.position;
                

                // Check if the center of the capsule is inside
                if (IsPointInsidePolygon(enemyPosition, transformedPoints))
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

                    if (IsCircleIntersectingLine(enemyPosition, capsuleRadius, edgeStart, edgeEnd))
                    {
                        validActors.Add(enemy);
                        break;
                    }
                }
            }

            return validActors;
        }

        public static bool IsPointInsidePolygon(Vector3 point, List<Vector3> polygon)
        {
            if (polygon == null || polygon.Count < 3)
            {
                Debug.LogWarning("Polygon must have at least 3 points.");
                return false;
            }

            int intersections = 0;

            for (int i = 0; i < polygon.Count; i++)
            {
                Vector3 vertex1 = polygon[i];
                Vector3 vertex2 = polygon[(i + 1) % polygon.Count]; // Wrap around to the first vertex

                // Check if the ray from the point to +X axis intersects the polygon edge
                if (IsIntersecting(point, vertex1, vertex2))
                {
                    intersections++;
                }
            }

            // If the number of intersections is odd, the point is inside the polygon
            return (intersections % 2) == 1;
        }

        private static bool IsIntersecting(Vector3 point, Vector3 vertex1, Vector3 vertex2)
        {
            // Ensure we work in 2D (XZ-plane)
            point.y = 0;
            vertex1.y = 0;
            vertex2.y = 0;

            // Check if the edge straddles the horizontal ray from the point
            if ((vertex1.z > point.z && vertex2.z <= point.z) || (vertex2.z > point.z && vertex1.z <= point.z))
            {
                // Compute the intersection point's X-coordinate
                float t = (point.z - vertex1.z) / (vertex2.z - vertex1.z);
                float intersectionX = vertex1.x + t * (vertex2.x - vertex1.x);

                // Check if the intersection is to the right of the point
                return intersectionX > point.x;
            }

            return false;
        }
        private bool IsCircleIntersectingLine(Vector3 circleCenter, float radius, Vector3 lineStart, Vector3 lineEnd)
        {
            // Project the circle center onto the line segment and find the closest point
            Vector3 lineDir = lineEnd - lineStart;
            float lineLength = lineDir.magnitude;
            lineDir.Normalize();

            Vector3 pointToCircle = circleCenter - lineStart;
            float t = Mathf.Clamp(Vector3.Dot(pointToCircle, lineDir), 0, lineLength);
            Vector3 closestPoint = lineStart + t * lineDir;

            // Check the distance from the closest point to the circle's center
            float distanceSquared = (closestPoint - circleCenter).sqrMagnitude;
            return distanceSquared <= radius * radius;
        }

        internal bool IsWithinRange(Actor actor, Actor target)
        {
            throw new NotImplementedException();
        }

        public float ApplyPostureModifier(float posture)
        {
            if(state == STATE.windup)
               return posture * windupPostureDamageMult;
            if(state == STATE.recovery)
                return posture * recoveryPostureDamageMult; 
            return posture;
        }
    }
}