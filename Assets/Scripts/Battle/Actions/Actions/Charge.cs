using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
{
    [CreateAssetMenu(fileName = "Charge", menuName = "ScriptableObjects/Action/Charge", order = 1)]
    public class Charge : BaseAction
    {
        public float power;
        private Vector3 chargeVector;
        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            casterActor.PlayAnimation("Dash", () =>
            {
                if (Tags.Contains(TAG.USESTICK))
                    chargeVector = Direction * 100 * power;
                else
                    chargeVector = casterActor.target.DirectionToClosestEnemy * 100 * power;
                casterActor.rigidbody.AddForce(chargeVector);

            }, onPerformEnd, true);
        }
    }
}