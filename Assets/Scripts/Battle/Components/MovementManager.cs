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

        public void FaceDirection(Vector3 direction)
        {
            //Face the enemy
            var lookrotation = direction.normalized;
            lookrotation.y = 0;
            actor.transform.rotation = Quaternion.LookRotation(lookrotation, Vector3.up);
        }

        public void FaceTarget(Actor target)
        {
            //Face the enemy
            var targetpoint = target.transform.position;
            var lookrotation = (targetpoint - actor.transform.position).normalized;
            lookrotation.y = 0;
           actor.transform.rotation = Quaternion.LookRotation(lookrotation, Vector3.up);
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
        public void MoveTowardTarget(Vector3 targetPoint, float acceleration, float maxSpeed)
        {
            // Calculate the desired direction and velocity
            Vector3 directionToTarget = (targetPoint - actor.transform.position);

            // Get the current velocity
            Vector3 currentVelocity = rigidbody.velocity;

            // Calculate the desired velocity (direction * max speed)
            Vector3 desiredVelocity = directionToTarget * maxSpeed;

            // Smoothly interpolate the velocity (acceleration defines how quickly it adjusts)
            Vector3 newVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);

            // Apply the new velocity
            rigidbody.velocity = newVelocity;

            // Face the direction of movement
            if (newVelocity.magnitude > 0.01f)
            {
                FaceDirection(newVelocity.normalized);
            }
        }

        public void ChangeSpeed(Vector3 force)
        {

            // Apply the force and ensure the velocity is within a maximum value
            //var appliedForce = Mathf.Max(force.magnitude, rigidbody.velocity.magnitude);
            rigidbody.AddForce(force, ForceMode.VelocityChange);

            // Get the direction from the velocity (this will be a normalized vector pointing in the direction of movement)
            Vector3 velocityDirection = rigidbody.velocity.normalized;

            // Face the direction the object is moving (using the velocity direction)
            if (velocityDirection.magnitude > 0.01f) // Prevent rotating when velocity is almost zero
            {
                FaceDirection(velocityDirection); // Point to the target position based on velocity direction
            }

            // Limit the maximum speed of the object
            float maxSpeed = 5f;
            if (rigidbody.velocity.magnitude > maxSpeed)
            {
                rigidbody.velocity = rigidbody.velocity.normalized * maxSpeed;
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