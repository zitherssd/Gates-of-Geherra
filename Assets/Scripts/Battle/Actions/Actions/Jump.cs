using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Actions
{
    [CreateAssetMenu(fileName = "Jump", menuName = "ScriptableObjects/Action/Jump", order = 1)]
    public class Jump : BaseAction
    {
        public float power;

        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            var direction = GetRelativeToCamera(StickValue);
            direction.y = power/1.5f * StickValue.magnitude;
            casterActor.rigidbody.AddForce(direction * 100);
            casterActor.transform.position = new Vector3(casterActor.transform.position.x, casterActor.transform.position.y + 0.011f, casterActor.transform.position.z);
            float dotProduct = Vector3.Dot(Camera.main.transform.right, direction.normalized);
            if (dotProduct > 0)
                casterActor.state.TransitionTo(casterActor.state.airNeutralState);
            else
                casterActor.state.TransitionTo(casterActor.state.rollState);
            casterActor.StartCoroutine(casterActor.WaitForTime(onPerformEnd, 1f));
        }

        private Vector3 GetRelativeToCamera(Vector2 direction)
        {
            var camera = Camera.main;
            var forward = camera.transform.forward; forward.y = 0;
            var right = camera.transform.right; right.y = 0;
            forward.Normalize(); right.Normalize();

            var desiredMoveDirection = forward * direction.y + right * direction.x;
            return desiredMoveDirection;
        }
    }
}