using Assets.Scripts.Actions;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
{
    [CreateAssetMenu(fileName = "Charge", menuName = "ScriptableObjects/Action/Charge", order = 1)]
    public class Charge : BaseSkill
    {
        public float power;
        private Vector3 chargeVector;
        private Actor casterActor;
        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            this.casterActor = casterActor;
            casterActor.state.TransitionTo(casterActor.state.actingState.Set(this, onPerformEnd));
        }

        public override void OnHit()
        {
            if (Tags.Contains(TAG.USESTICK))
                chargeVector = Direction * StickMult;
            else
                chargeVector = casterActor.target.DirectionToClosestEnemy * StickMult;

            casterActor.movementForce = chargeVector * power;
        }

    }
}