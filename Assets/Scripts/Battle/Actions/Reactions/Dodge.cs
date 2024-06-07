using Assets.Scripts.Actions;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Reactions
{
    [CreateAssetMenu(fileName = "Dodge", menuName = "ScriptableObjects/Reaction/Dodge")]

    public class Dodge : BaseReaction
    {
        [Range(0, 10)]
        public float force;
        public float time;
        public ANIMATION Animation;
        public TYPE Type;
        private LTDescr moveTween;
        private Actor actor;

        protected override void PerformSpecific(Actor actor, Action onReactionComplete)
        {
            this.actor = actor;
            actor.KillAnimationEndEvent();
            if (actor.isControllable())
            {
                var desiredMoveDirection = GetRelativeToCamera(StickValue);
                actor.PlayAnimation(Animation.ToString());
                if (Type == TYPE.Normal)
                {
                    actor.GetComponent<Rigidbody>().AddForce(desiredMoveDirection.normalized * 100 * force);
                }
                else if (Type == TYPE.Fast)
                {
                    moveTween = LeanTween.move(actor.gameObject, actor.transform.position + desiredMoveDirection.normalized * force, time).setEaseOutExpo().setOnUpdate(OnTweenUpdate).setOnComplete(() => TransitionToIdle(actor));
                }
            }
            else
            {
                var sign = 0;
                if ((int)UnityEngine.Random.Range(0, 2) == 0) { sign = -1; } else { sign = 1; };

                var aux = new Vector2(0, sign);
                var desiredMoveDirection = GetRelativeToCamera(aux);
                actor.PlayAnimation(Animation.ToString());
                if (Type == TYPE.Normal)
                {
                    actor.GetComponent<Rigidbody>().AddForce(desiredMoveDirection.normalized * 100 * force);
                }
                else if (Type == TYPE.Fast)
                {
                    moveTween = LeanTween.move(actor.gameObject, actor.transform.position + desiredMoveDirection.normalized * force, time).setEaseOutExpo().setOnUpdate(OnTweenUpdate).setOnComplete(() => TransitionToIdle(actor));
                }
            }
        }

        private void TransitionToIdle(Actor actor)
        {
            actor.state.TransitionTo(actor.state.idleState);
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

        private void OnTweenUpdate(float tweenValue)
        {
            if (actor.rigidbody.velocity.magnitude > 0.01f)
            {
                LeanTween.cancel(moveTween.uniqueId);
                Debug.Log("Tween cancelled because the actor's velocity magnitude is greater than 0.1f.");
            }
        }
    }

    public enum ANIMATION { Step, Roll }
    public enum TYPE { Normal, Fast }

}
