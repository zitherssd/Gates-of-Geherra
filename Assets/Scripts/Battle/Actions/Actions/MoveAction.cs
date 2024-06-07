using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
{
    [CreateAssetMenu(fileName = "Move", menuName = "ScriptableObjects/Action/Move", order = 1)]
    public class MoveAction : BaseAction
    {
        public static MoveAction instance;

        public override void Perform(Actor casterActor, Action onPerformEnd)
        {
            UpdateRemainingUses();
            ResetCooldown();

            if (!Tags.Contains(TAG.USESLIDER)) SliderValue = 1f;
            if (Tags.Contains(TAG.KILLMOMENTUM)) casterActor.GetComponent<Rigidbody>().velocity = Vector3.zero;

            casterActor.ActorData.DealStaminaDamage(StaminaCost);
            casterActor.ActorData.ChangeBuildup(BuildupGain);
            casterActor.ActorData.ChangeBuildup(-BuildupCost);

            casterActor.state.TransitionTo(casterActor.state.actingState);

            if (Tags.Contains(TAG.REPEAT_TURN))
            {
                PerformSpecific(casterActor, () =>
                {
                    casterActor.Act(onPerformEnd);
                });
            }
            else
                PerformSpecific(casterActor, onPerformEnd);
        }

        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            if (casterActor.isControllable())
            {
                var move = GetRelativeToCamera(StickValue);
                var TargetPosition = casterActor.transform.position + (move * casterActor.ActorData.AGI);

                casterActor.state.TransitionTo(casterActor.state.moveState.Set(TargetPosition, onPerformEnd));
            }
            else
            {
                //casterActor.Move(new Vector3(StickValue.x, 0, StickValue.y), onPerformEnd);



                var move = new Vector3(StickValue.x, 0, StickValue.y);
                var TargetPosition = casterActor.transform.position + (move * casterActor.ActorData.AGI);

                casterActor.state.TransitionTo(casterActor.state.moveState.Set(TargetPosition, onPerformEnd));
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

        private void Awake()
        {
            //StickMult = 4f;
        }
    }
}