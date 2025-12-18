using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Battle.Actions;

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
                public float windupPostureDamageMult = 1f;
                public float recoveryPostureDamageMult = 1f;
                public float BuildupGainOnHit;
                public float StrengthScaling = 0f;
                public float AgilityScaling = 0f;
                public float MindScaling = 0f;
        
        
        
        
                protected override void PerformSpecific(Actor.Actor casterActor, Action onPerformEnd)
                {
                    //1. Get Target
                    //2. Continuously Rotate Incrementally so you are facing target //more work
                    //3. Move towards the Direction
                    var actingState = casterActor.state.TransitionTo<ActingState>();
        
                    actingState.Set(this);
        
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
                    casterActor.OnBeforeTakeDamage += ApplyPostureModifier;
                }
                public void Unsubscribe()
                {
                    _caster.OnBeforeTakeDamage -= ApplyPostureModifier;
                }
                public void ApplyDamageEffects(Actor.Actor casterActor, Actor.Actor targetActor, Action onDamageEffectsApplied)
                {
                    var damageInstance = new DamageInstance
                    {
                        Damage = this.Damage,
                        PostureDamage = this.PostureDamage,
                        KnockbackForce = this.KnockbackForce,
                        StrengthScaling = this.StrengthScaling,
                        AgilityScaling = this.AgilityScaling,
                        MindScaling = this.MindScaling,
                        BuildupGainOnHit = this.BuildupGainOnHit,
                    };
        
                    casterActor.DealDamage(damageInstance, targetActor, this);
                    
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
                    return false;
                }
        
        
                public override void OnHit()
                {
                    //RaycastHit hit;
                    //var targetDir = target.transform.position - _caster.transform.position;
                    _caster.effects.ClearHitbox();
        
        
                    var hits = CheckEnemiesInsideHitbox();
                    foreach (var hit in hits)
                    {
                        if (Tags.Contains(TAG.TECH) && !target.state.IsBlocking())
                        {
                            ApplyDamageEffects(_caster, hit, null);
                            this.EndAction(ActionEndReason.Completed);
                        }
                        else
                        {
                            ApplyDamageEffects(_caster, hit, null);
                        }
                        _caster.ActorData.ChangeBuildup(BuildupGainOnHit);
                        if(_caster.isControllable) UIManager.instance.GainMeter(SlowdownMeterGain);
                    }
                }
        
                public override void OnEnterWindup(Animator animator)
                {
                    _caster.effects.SetHitbox(HitboxPoints.Select(point => point * HitboxScale).ToList());
                    CalculateDuration(animator);
                    animator.speed = windupTimeMult;
                    state = STATE.windup;
                }
        
                public override void OnEnterRecovery(Animator animator)
                {
                    animator.speed = recoveryTimeMult;
                    state = STATE.recovery;
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
                        Vector3 worldPoint = _caster.transform.position + _caster.transform.TransformDirection(point * HitboxScale);
                        transformedPoints.Add(worldPoint);
                    }
        
                    List<Actor.Actor> enemyActors = _caster.isControllable
                        ? BattleManager.instance.EnemyActors
                        : BattleManager.instance.PlayerActors;


            foreach (var enemy in enemyActors.Where(actor => actor.state.IsAlive()))
            {
                Vector3 enemyPosition = enemy.transform.position;


                // Check if the center of the capsule is inside
                if (Intersections.IsPointInsidePolygon(enemyPosition, transformedPoints))
                {
                    if (!enemy.state.IsKnockedDown())
                    {
                        validActors.Add(enemy);
                        continue;
                    }
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

        public void ApplyPostureModifier(DamageInstance damageInstance)
        {
            if (state == STATE.windup)
                damageInstance.PostureDamage *= windupPostureDamageMult;
            if (state == STATE.recovery)
                damageInstance.PostureDamage *= recoveryPostureDamageMult;
        }
    }
}