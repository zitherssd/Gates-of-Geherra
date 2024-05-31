using Assets.Scripts.Battle;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class ProjectileHandler : MonoBehaviour
    {
        public float Damage;
        public float PostureDamage;
        public float KnockbackForce;

        private Rigidbody rb;


        public void Start()
        {
            rb = gameObject.GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision other)
        {
            Debug.Log("Entered trigger zone with: " + other.gameObject.name);

            Actor enemyBattler = other.gameObject.GetComponent<Actor>();


            if (enemyBattler != null)
            {
                ApplyDamageEffects(enemyBattler);
                Destroy(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public void ApplyDamageEffects(Actor targetActor)
        {
            // Apply Damage
            var damage = Damage - targetActor.ActorData.DEF;
            if (damage > 0)
            {
                targetActor.ApplyDamage(damage);
            };

            //Apply posture
            if (PostureDamage > 0)
            {
                targetActor.ApplyPosture(PostureDamage);
            }

            // Apply Knockback
            if (KnockbackForce > 0)
            {
                var direction = rb.velocity.normalized;
                targetActor.ApplyKnockback(direction, KnockbackForce);
            }
        }
    }
}