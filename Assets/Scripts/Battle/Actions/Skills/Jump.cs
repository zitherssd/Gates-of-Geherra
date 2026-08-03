using Assets.Scripts.Battle.Actor.States;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "Jump", menuName = "ScriptableObjects/Action/Jump", order = 1)]
    public class Jump : BaseAction
    {
        public float power;
        public float vertpower;

        protected override void PerformSpecific(Actor.Actor casterActor, Action onPerformEnd)
        {
            var direction = Direction * power;
            direction.y = vertpower;
            casterActor.movement.AddForce(direction);
            casterActor.transform.position = new Vector3(casterActor.transform.position.x, casterActor.transform.position.y + 0.011f, casterActor.transform.position.z);
            float dotProduct = Vector3.Dot(casterActor.transform.forward, Direction);
            if (dotProduct > 0)
                casterActor.state.TransitionTo<AirNeutralState>();
            else
                casterActor.state.TransitionTo<RollState>();
            onPerformEnd?.Invoke();
            //casterActor.StartCoroutine(casterActor.WaitForTime(onPerformEnd, 1f));
        }
    }
}