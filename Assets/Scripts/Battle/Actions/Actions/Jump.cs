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
            var direction = Direction * power;
            direction.y = power/1.3f * Direction.magnitude;
            casterActor.movement.AddForce(direction);
            casterActor.transform.position = new Vector3(casterActor.transform.position.x, casterActor.transform.position.y + 0.011f, casterActor.transform.position.z);
            float dotProduct = Vector3.Dot(casterActor.transform.forward, Direction);
            if (dotProduct > 0)
                casterActor.state.TransitionTo(casterActor.state.airNeutralState);
            else
                casterActor.state.TransitionTo(casterActor.state.rollState);
            casterActor.StartCoroutine(casterActor.WaitForTime(onPerformEnd, 1f));
        }
    }
}