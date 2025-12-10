using Assets.Scripts.Battle.Actor.States;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Actions
{
    [CreateAssetMenu(fileName = "Charge", menuName = "ScriptableObjects/Action/Charge", order = 1)]
    public class Charge : BaseSkill
    {
        public float power;
        private Vector3 chargeVector;
        private Actor.Actor casterActor;
        protected override void PerformSpecific(Actor.Actor casterActor, Action onPerformEnd)
        {
            this.casterActor = casterActor;

            var state = casterActor.state.TransitionTo<ActingState>();
            state.Set(this);
        }

        protected override void Cleanup(ActionEndReason reason)
        {
            base.Cleanup(reason);
            if (reason == ActionEndReason.Completed)
            {
                if (casterActor.isControllable)
                {
                    UIManager.instance.GainMeter(SlowdownMeterGain);
                }
                casterActor.movement.SetFriction();
            }
        }

        public override void OnHit()
        {
            casterActor.movement.SetFriction(0);
            if (Tags.Contains(TAG.USESTICK))
            {
                casterActor.movement.AddForce(Direction * power * StickMult);
                casterActor.movement.FaceDirection(Direction);
            }
            else
            {
                casterActor.movement.AddForce(casterActor.target.DirectionToClosestEnemy * power);
                casterActor.movement.FaceDirection(casterActor.target.DirectionToClosestEnemy);

            }
        }

    }
}