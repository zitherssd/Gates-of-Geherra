using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
{
    [CreateAssetMenu(fileName = "Move", menuName = "ScriptableObjects/Action/Move", order = 1)]
    public class MoveAction : BaseAction
    {
        public static MoveAction instance;

        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            StickMult = 1 + casterActor.ActorData.AGI / 3;
            var scaledStickMult = Direction * (1 + casterActor.ActorData.AGI / 3);
            var TargetPosition = casterActor.transform.position + scaledStickMult;

            casterActor.state.TransitionTo(casterActor.state.moveState.Set(TargetPosition, onPerformEnd));

        }
    }
}