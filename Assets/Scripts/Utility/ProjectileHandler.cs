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
            var damageInstance = new DamageInstance
            {
                Damage = action.Damage,
                PostureDamage = action.PostureDamage,
                KnockbackForce = action.KnockbackForce,
            };

            targetActor.ApplyDamageInstance(damageInstance, casterActor, action);
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