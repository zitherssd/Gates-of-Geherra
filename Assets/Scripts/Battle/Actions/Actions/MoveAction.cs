using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
{
    [CreateAssetMenu(fileName = "Move", menuName = "ScriptableObjects/Action/Move", order = 1)]
    public class MoveAction : BaseAction
    {
        public static MoveAction instance;
        public float duration;

        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            OnCancel += onPerformEnd;
            StickMult = 1 + casterActor.ActorData.AGI / 10;
            var scaledStickMult = Direction * StickMult;
            var TargetPosition = casterActor.transform.position + scaledStickMult;

            if (Type == BUTTONTYPE.CONTINUOUS_VECTOR)
            {
                casterActor.state.TransitionTo(casterActor.state.moveState.Set(duration, OnCancel, this));
            }
            else
                casterActor.state.TransitionTo(casterActor.state.moveState.Set(TargetPosition, onPerformEnd));
        }
    }
}