using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Utility;
using System;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.Battle.Actions.BaseAction;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    public enum KnockbackType
    {
        DIRECTION,
        AWAY,
        BACK,
        FRONT,
        UP_ONLY,
        UP_AND_AWAY,
        UP_AND_DIRECTION,
    }

    [Serializable]
    public class DamageInstance
    {
        public float Damage;
        public float PostureDamage;
        public float KnockbackForce;
        public float KnockbackForceUp;
        public KnockbackType KnockbackType;
        [Range(0, 10)] public float StrengthScaling;
        [Range(0, 10)] public float AgilityScaling;
        [Range(0, 10)] public float MindScaling;
        public float BuildupGainOnHit;

        public (float Damage, float PostureDamage, Vector3 Knockback, float KnockbackForce) Calculate (Actor.Actor casterActor, Actor.Actor targetActor, BaseAction action)
        {
            var damage = Damage + casterActor.ActorData.Strength * StrengthScaling - targetActor.ActorData.Strength * StrengthScaling / 2 + casterActor.ActorData.Agility * AgilityScaling - targetActor.ActorData.Agility * AgilityScaling / 2 + casterActor.ActorData.Mind * MindScaling;

            Vector3 direction;
            switch(KnockbackType)
            {
                case KnockbackType.DIRECTION:
                    direction = action.Direction.normalized;
                    break;
                case KnockbackType.AWAY:
                    direction = (targetActor.transform.position - casterActor.transform.position).normalized;
                    break;
                case KnockbackType.BACK:
                    direction = action.Direction.normalized.normalized;
                    Vector3 cameraForward = Camera.main.transform.forward;
                    Vector3 aux = Vector3.Cross(direction, -Vector3.up);

                    if (Vector3.Dot(aux, cameraForward) < 0f) //if its oppsoite the camera
                    {
                        aux = -aux; //make it face the camera
                    }
                    direction += aux;
                    break;
                case KnockbackType.FRONT:
                    direction = action.Direction.normalized.normalized;
                    cameraForward = Camera.main.transform.forward;
                    aux = Vector3.Cross(direction, Vector3.up);

                    if (Vector3.Dot(aux, cameraForward) < 0f) //if its oppsoite the camera
                    {
                        aux = -aux; //make it face the camera
                    }
                    direction += aux;
                    break;
                case KnockbackType.UP_ONLY:
                    direction = Vector3.up;
                    break;
                case KnockbackType.UP_AND_AWAY:
                    direction = ((targetActor.transform.position - casterActor.transform.position).normalized + Vector3.up).normalized;
                    break;
                case KnockbackType.UP_AND_DIRECTION:
                    direction = (action.Direction.normalized.normalized + Vector3.up).normalized;
                    break;
                default:
                    direction = Vector3.zero;
                    break;
            }
            var knockback = direction;

            return (damage, PostureDamage, knockback, KnockbackForce);
        }
    }


    [Serializable]
    public class DamageEffect : IEffect, IItemEffect
    {
        public bool UseHitbox = true;
        public IHitbox HitboxEffect; //How to get hitbox?
        public DamageInstance DamageData;


        public virtual void Eval(Actor.Actor actor, BaseAction action)
        {
            var gs = action as GenericSkill;
            HitboxEffect = gs.OnStartEffects.Where(item => item is IHitbox).FirstOrDefault() as IHitbox;
            var enemies = HitboxEffect.CheckEnemiesInsideHitbox(actor);
            foreach (var enemy in enemies)
                ApplyDamageEffects(actor, enemy, action);
        }

        public void Eval(Actor.Actor actor)
        {
            ApplyDamageEffects(actor, actor, null);
        }

        public void ApplyDamageEffects(Actor.Actor casterActor, Actor.Actor targetActor, BaseAction action)
        {
            // Need ro revise
            var targetBlocking = targetActor.state.IsBlocking();
            if (casterActor.isControllable && !targetBlocking)
                UIManager.instance.GainMeter(action.SlowdownMeterGain);
            if (targetBlocking)
                casterActor.ActorData.ChangeBuildup(DamageData.BuildupGainOnHit / 2);
            else
                casterActor.ActorData.ChangeBuildup(DamageData.BuildupGainOnHit);

            var (dmg, postureDmg, direction, force) = DamageData.Calculate(casterActor, targetActor, action);

            targetActor.ApplyDamageInstance(dmg, postureDmg, direction, force);
        }
    }
}
