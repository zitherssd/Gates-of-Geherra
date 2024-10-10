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
            casterActor.state.TransitionTo(casterActor.state.actingState.Set(this, () => { casterActor.movement.SetFriction(); onPerformEnd?.Invoke(); }));
        }

        public override void OnHit()
        {
            casterActor.movement.SetFriction(0);
            if (Tags.Contains(TAG.USESTICK))
            {
                casterActor.movement.AddForce(Direction * power * StickMult);
            }
            else
            {
                casterActor.movement.AddForce(casterActor.target.DirectionToClosestEnemy * power);
            }
        }

    }
}