using Assets.Scripts.Battle;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Actions.Effects;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Actor;
using Unity.VisualScripting;
using UnityEngine;
using static Assets.Scripts.Battle.Actions.BaseAction;

namespace Assets.Scripts.Utility
{
    public class ProjectileHandler : MonoBehaviour
    {
        private Actor owner;
        private ProjectileAttack action;
        private Rigidbody rb;
        [SerializeField] private Transform vfxRoot;

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
            if (other.gameObject == owner.gameObject) return;
            Actor enemyBattler = other.gameObject.GetComponent<Actor>();

            // --- Detach travel particles BEFORE destroying the projectile ---
            DetachAndLetVFXFinish();

            if (enemyBattler != null)
            {
                if (!enemyBattler.state.IsAlive()) return;
                action.OnProjectileHitEffects.ForEach(effect => effect.Eval(owner, action, this.gameObject));
                action.OnHitEffects.ForEach(effect => effect.Eval(owner, action));
                ApplyDamageEffects(owner, enemyBattler, action);
            }

            Destroy(this.gameObject);
        }

        public void ApplyDamageEffects(Actor casterActor, Actor targetActor, ProjectileAttack action)
        {
            // Need ro revise
            var targetBlocking = targetActor.state.IsBlocking();
            if (casterActor.isControllable && !targetBlocking)
                UIManager.instance.GainMeter(action.SlowdownMeterGain);


            // Apply Damage
            var damage = action.Damage;

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

            targetActor.ApplyDamageInstance(damage, action.PostureDamage, direction, action.KnockbackForce);
        }

        private void DetachAndLetVFXFinish()
        {
            if (vfxRoot == null) return;

            // Detach the particle VFX from projectile
            vfxRoot.SetParent(null, true);

            // Destroy all particle systems after they finish
            foreach (var ps in vfxRoot.GetComponentsInChildren<ParticleSystem>())
            {
                float maxLifetime = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(ps.gameObject, maxLifetime);
                ps.Stop();  // prevents looping particles from staying alive forever
            }

            // Handle Trail Renderers too
            foreach (var tr in vfxRoot.GetComponentsInChildren<TrailRenderer>())
            {
                tr.autodestruct = true;
                tr.transform.parent = null;
            }
        }
    }
}