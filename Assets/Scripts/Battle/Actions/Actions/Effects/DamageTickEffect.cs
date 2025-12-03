using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets.Scripts.Battle.Actions.BaseAction;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class DamageTickEffect : IEffect, IUpdateableEffect
    {
        public ShowHitboxOnPlayer HitboxEffect;
        public float tickRate = 0.1f;
        private float timer;
        public float PostureDamage;
        public float Damage;
        public float KnockbackForce;
        public float CasterBuildupGain;
        public float StrengthScaling;
        public float AgilityScaling;
        public float MindScaling;

        public void Eval(Actor.Actor actor, BaseAction action)
        {
            timer = 0f;
        }

        public void Update(Actor.Actor actor, BaseAction action, float dt)
        {
            timer += dt;
            if (timer >= tickRate)
            {
                timer -= tickRate;

                var enemies = HitboxEffect.CheckEnemiesInsideHitbox(actor);
                foreach (var enemy in enemies)
                    ApplyDamageEffects(actor, enemy, action);
            }

        }

        private void ApplyDamageEffects(Actor.Actor casterActor, Actor.Actor targetActor, BaseAction action)
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


            if (action.Tags.Contains(TAG.KNOCKBACK_AIR)) { direction = (direction + Vector3.up).normalized; }

            targetActor.ApplyDamageInstance(damage, PostureDamage, direction, KnockbackForce);
        }
    }
}
