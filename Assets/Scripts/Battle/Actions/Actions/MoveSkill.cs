using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
{
    [CreateAssetMenu(fileName = "Move", menuName = "ScriptableObjects/Action/Move", order = 1)]
    public class MoveSkill : BaseAction
    {
        public static MoveSkill instance;

        public override void Perform(BaseActorBattler casterActor, Action onPerformEnd)
        {
            if(casterActor.isControllable())
            {
                casterActor.Move(GetRelativeToCamera(StickValue), onPerformEnd);
            }
            else
            {
                casterActor.Move(new Vector3(StickValue.x, 0, StickValue.y), onPerformEnd);
            }
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