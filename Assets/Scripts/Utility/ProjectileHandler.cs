using Assets.Scripts.Battle;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Actions.Effects;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Actor;
using UnityEngine;
using static Assets.Scripts.Battle.Actions.BaseAction;

namespace Assets.Scripts.Utility
{
    public class ProjectileHandler : MonoBehaviour
    {
        private Actor owner;
        private ProjectileAttack action;
        private Rigidbody rb;

        public void Start()
        {
            rb = gameObject.GetComponent<Rigidbody>();
        }

        public void Initialize(Actor owner, ProjectileAttack action)
        {
            this.owner = owner;
            this.action = action;
            transform.localScale = transform.localScale * this.action.SizeMultiplier;
        }

        private void OnTriggerEnter(Collider other)
        {

            Actor enemyBattler = other.gameObject.GetComponent<Actor>();


            if (enemyBattler != null)
            {
                action.OnProjectileHitEffects.ForEach(effect => effect.Eval(owner, action, this.gameObject));
                action.OnHitEffects.ForEach(effect => effect.Eval(enemyBattler, action));
                ApplyDamageEffects(owner, enemyBattler, action);
                Destroy(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public void ApplyDamageEffects(Actor casterActor, Actor targetActor, ProjectileAttack action)
        {
            // Applyv  Knockback
            if (action.KnockbackForce > 0)
            {
                Vector3 direction;
                if (action.Tags.Contains(TAG.USESTICK))
                    direction = action.Direction.normalized;
                else
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
                targetActor.ApplyKnockback(direction, action.KnockbackForce);
            }

            //Apply posture
            if (action.PostureDamage > 0)
            {
                targetActor.ApplyPosture(action.PostureDamage);
                if (casterActor.isControllable && !targetActor.state.IsBlocking())
                {
                    UIManager.instance.GainMeter(action.SlowdownMeterGain);
                }
            }

            // Apply Damage
            var damage = action.Damage + casterActor.ActorData.ATK - targetActor.ActorData.DEF;
            if (damage > 0)
            {
                var hitstop = StaticHelpers.LinearMap(damage, 0.2f, 15, 0.083f, 0.420f);
                targetActor.ApplyDamage(damage);
            };
        }
    }
}