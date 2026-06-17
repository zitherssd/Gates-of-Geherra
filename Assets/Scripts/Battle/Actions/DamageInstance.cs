using Assets.Scripts.Battle.Actor;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
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

        public DamageInstanceResult Calculate(Actor.Actor casterActor, Actor.Actor targetActor, BaseAction action)
        {
            var damage = Damage + casterActor.Runtime.Strength * StrengthScaling - targetActor.Runtime.Strength * StrengthScaling / 2 + casterActor.Runtime.Agility * AgilityScaling - targetActor.Runtime.Agility * AgilityScaling / 2 + casterActor.Runtime.Mind * MindScaling;

            Vector3 direction;
            switch (KnockbackType)
            {
                case KnockbackType.DIRECTION:
                    direction = action != null ? action.Direction.normalized : Vector3.zero;
                    break;
                case KnockbackType.AWAY:
                    direction = (targetActor.transform.position - casterActor.transform.position).normalized;
                    break;
                case KnockbackType.BACK:
                    direction = action != null ? action.Direction.normalized : Vector3.zero;
                    Vector3 cameraForward = Camera.main.transform.forward;
                    Vector3 aux = Vector3.Cross(direction, -Vector3.up);

                    if (Vector3.Dot(aux, cameraForward) < 0f) //if its oppsoite the camera
                    {
                        aux = -aux; //make it face the camera
                    }
                    direction += aux;
                    break;
                case KnockbackType.FRONT:
                    direction = action != null ? action.Direction.normalized : Vector3.zero;
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
                    direction = (action != null ? action.Direction.normalized : Vector3.zero + Vector3.up).normalized;
                    break;
                default:
                    direction = Vector3.zero;
                    break;
            }
            var knockback = direction;

            return new DamageInstanceResult
            {
                DamageDealt = damage,
                PostureDamageDealt = PostureDamage,
                KnockbackApplied = knockback,
                KnockbackForceApplied = KnockbackForce,
            };
        }
    }

    public class DamageInstanceResult
    {
        public float DamageDealt;
        public float PostureDamageDealt;
        public Vector3 KnockbackApplied;
        public float KnockbackForceApplied;
    }
}
