using Assets.Scripts.Actions;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Reactions
{
    [CreateAssetMenu(fileName = "Dodge", menuName = "ScriptableObjects/Reaction/Dodge")]

    public class Dodge : BaseReaction
    {
        [Range(0,10)]
        public float force;
        public ANIMATION Animation;

        protected override void PerformSpecific(Actor actor, Action onReactionComplete)
        {
            actor.KillAnimationEndEvent();
            if(actor.isControllable())
            {
                var desiredMoveDirection = GetRelativeToCamera(StickValue);
                actor.PlayAnimation(Animation.ToString());
                actor.GetComponent<Rigidbody>().AddForce(desiredMoveDirection.normalized * 100 * force);
            }
            else
            {
                var aux = new Vector2(0, 1f);
                if ((int)UnityEngine.Random.Range(0, 2) == 0)
                    aux.y = -aux.y;

                var desiredMoveDIrection = GetRelativeToCamera(StickValue);
                actor.PlayAnimation(Animation.ToString());
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

        public enum ANIMATION { Step, Roll }

    }
}