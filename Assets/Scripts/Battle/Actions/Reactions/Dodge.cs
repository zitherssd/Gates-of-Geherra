using Assets.Scripts.Actions;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Reactions
{
    [CreateAssetMenu(fileName = "Dodge", menuName = "ScriptableObjects/Reaction/Dodge")]

    public class Dodge : BaseSkill
    {
        [Range(0, 10)]
        public float force;
        public float time;
        public TYPE Type;
        private LTDescr moveTween;
        private Actor actor;

        protected override void PerformSpecific(Actor actor, Action onActionComplete)
        {
            CameraManager.instance.SlowTrack = true;
            this.actor = actor;


            if (Type == TYPE.Force)
            {
                var chargeVector = Direction.normalized * StickMult * force;

                actor.movementForce = chargeVector;

                actor.state.TransitionTo(actor.state.actingState.Set(this, onActionComplete));

            }
            else if (Type == TYPE.Lerp)
            {
                actor.Move(actor.transform.position + Direction * StickMult, time, LeanTweenType.easeOutCirc);
                actor.state.TransitionTo(actor.state.actingState.Set(this, onActionComplete));
            }

            //IF AI
            //var sign = 0;
            //if ((int)UnityEngine.Random.Range(0, 2) == 0) { sign = -1; } else { sign = 1; };

            //var aux = new Vector2(0, sign);
            //var desiredMoveDirection = GetRelativeToCamera(aux);
            //if (Type == TYPE.Force)
            //{
            //    actor.GetComponent<Rigidbody>().AddForce(desiredMoveDirection.normalized * 100 * force);
            //    actor.PlayAnimation(Animation.ToString(),onActionComplete);
            //}
            //else if (Type == TYPE.Lerp)
            //{
            //    actor.PlayAnimation(Animation.ToString(), onActionComplete);
            //    moveTween = LeanTween.move(actor.gameObject, actor.transform.position + desiredMoveDirection.normalized * force, time).setEaseOutExpo().setOnUpdate(OnTweenUpdate);
            //}
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
    public enum TYPE { Force, Lerp }

}
