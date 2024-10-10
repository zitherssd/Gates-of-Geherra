using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components
{
    public class MovementManager
    {
        private readonly Actor actor;
        private readonly Rigidbody rigidbody;
        private readonly Collider collider;
        private PhysicMaterial physicMaterial;
        private readonly float normalFriction = 0.5f;
        public bool ApplyForces;
        public bool ApplyGravity;
        

        public MovementManager(Actor actor)
        {
            this.actor = actor;
            collider = actor.GetComponent<Collider>();
            rigidbody = actor.GetComponent<Rigidbody>();
            if (collider.material == null)
            {
                physicMaterial = new PhysicMaterial();
                physicMaterial.name = "DynamicFrictionMaterial";
                collider.material = physicMaterial;
            }
            else
            {
                physicMaterial = collider.material;
            }

            // Set initial friction values
            physicMaterial.dynamicFriction = 0.5f;
            physicMaterial.staticFriction = 0;
            physicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        }

        public void AddForce(Vector3 force)
        {
            var currentMagnitude = rigidbody.velocity.magnitude;
            rigidbody.AddForce(force, ForceMode.Impulse);
            Vector3 newVelocity = rigidbody.velocity;

            float maxSpeed = 5f;
            if (newVelocity.magnitude > maxSpeed)
            {
                // Clamp the velocity to the max speed while maintaining the direction
                rigidbody.velocity = newVelocity.normalized * currentMagnitude;
            }
        }

        public void ResetMomentum()
        {
            rigidbody.velocity = Vector3.zero;
        }

        public void SetFriction(float friction)
        {
            physicMaterial.dynamicFriction = friction;
        }

        public void SetFriction()
        {
            physicMaterial.dynamicFriction = normalFriction;

        }
    }
}