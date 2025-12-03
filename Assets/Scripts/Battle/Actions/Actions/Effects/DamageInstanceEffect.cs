using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Utility;
using System;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.Battle.Actions.BaseAction;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class HealEffect : IItemEffect
    {
        public void Eval(Actor.Actor owner)
        {
            owner.ActorData.HealAllBars();
        }
    }



    [Serializable]
    public class DamageInstanceEffect : IEffect, IItemEffect
    {
        public IHitbox HitboxEffect;
        public float PostureDamage;
        public float Damage;
        public float KnockbackForce;
        public float KnockbackForceUp;
        public float CasterBuildupGain;
        public bool AutoGetHitbox;
        public float StrengthScaling;
        public float AgilityScaling;
        public float MindScaling;



        public void Eval(Actor.Actor actor, BaseAction action)
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

        public void Eval(Actor.Actor owner, ItemEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void ApplyDamageEffects(Actor.Actor casterActor, Actor.Actor targetActor, BaseAction action)
        {


            // Need ro revise
            var targetBlocking = targetActor.state.IsBlocking();
            if (casterActor.isControllable && !targetBlocking)
                UIManager.instance.GainMeter(action.SlowdownMeterGain);
            if (targetBlocking)
                casterActor.ActorData.ChangeBuildup(CasterBuildupGain / 2);
            else
                casterActor.ActorData.ChangeBuildup(CasterBuildupGain);

            // Apply Damage
            var damage = Damage + casterActor.ActorData.Strength * StrengthScaling - targetActor.ActorData.Strength * StrengthScaling / 2 + casterActor.ActorData.Agility * AgilityScaling - targetActor.ActorData.Agility * AgilityScaling / 2 + casterActor.ActorData.Mind * MindScaling;

            Vector3 direction;
            if (action.Type is BUTTONTYPE.VECTOR)
                direction = action.Direction.normalized;
            else
                direction = (targetActor.transform.position - casterActor.transform.position).normalized;
            if (action.Tags.Contains(TAG.KNOCKBACK_AWAY))
                direction = (targetActor.transform.position - casterActor.transform.position).normalized;
            if (action.Tags.Contains(TAG.KNOCKBACK_BACK))
            {
                Vector3 cameraForward = Camera.main.transform.forward;
                Vector3 aux = Vector3.Cross(direction, -Vector3.up);

                if (Vector3.Dot(aux, cameraForward) < 0f) //if its oppsoite the camera
                {
                    aux = -aux; //make it face the camera
                }
                direction += aux;
            }

            if (action.Tags.Contains(TAG.KNOCKBACK_FRONT))
            {
                Vector3 cameraForward = Camera.main.transform.forward;
                Vector3 aux = Vector3.Cross(direction, Vector3.up);

                if (Vector3.Dot(aux, cameraForward) > 0f) //if it's the same as the camera
                {
                    aux = -aux; //make it opposite
                }
                direction += aux;
            }

            direction += Vector3.up * KnockbackForceUp;

            targetActor.ApplyDamageInstance(damage, PostureDamage, direction, KnockbackForce);
        }
    }
}
