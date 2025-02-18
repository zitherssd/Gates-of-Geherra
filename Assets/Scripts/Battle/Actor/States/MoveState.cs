using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Pattern;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State
{
    public class MoveState : IState
    {
        private Battle.Actor actor;
        private Vector3 destination;
        private Action onMoveComplete;
        private float sqrMag;
        private float lastSqrMag;
        private float minDistance = 0.1f;
        private float moveSpeed = 2f;
        private float duration;
        private bool continousAction;
        private float timer;
        public BaseAction action;


        public MoveState(Actor actor)
        {
            this.actor = actor;
        }

        public MoveState Set(float duration, Action onMoveComplete, BaseAction action)
        {
            this.duration = duration;
            this.onMoveComplete = onMoveComplete;
            this.action = action;
            timer = 0f;
            continousAction = true;
            return this;
        }

        public MoveState Set(Vector3 destination, Action onMoveComplete)
        {
            this.destination = destination;
            continousAction = false;
            this.onMoveComplete = onMoveComplete;
            return this;
        }

        public void Enter()
        {
            actor.audio.PlayAudio("Move2");
            actor.PlayAnimation("Run");
            lastSqrMag = Mathf.Infinity;
            //actor.movement.ResetMomentum();
            actor.movement.SetFriction(0);
            if (!continousAction)
            {
                actor.movement.AddForce((destination - actor.transform.position).normalized * (1 + actor.ActorData.AGI / 10));
                actor.movement.FaceDirection(destination - actor.transform.position);
            }
        }

        public void Exit()
        {
            onMoveComplete = null;
            actor.movement.SetFriction();
            actor.GetAnimator().speed = 1;

        }

        public void OnCollisionEnter(Collision collision)
        {
            if (continousAction)
            {
                onMoveComplete.Invoke();
            }
        }

        public void Update()
        {
            if (continousAction)
            {
                timer += Time.deltaTime;
                if (timer < 1f)
                {
                    var animator = actor.GetAnimator();
                    animator.speed = timer * 2 / 3 * (1 + (float)actor.ActorData.AGI/10);
                    //actor.movement.ChangeSpeed((action.Direction.normalized) * (1 + actor.ActorData.AGI/10) * timer);
                    actor.movement.MoveTowardTarget(action.Direction + actor.transform.position, 2f * timer, 1 + (float)actor.ActorData.AGI / 10);
                }
                else
                    //actor.movement.ChangeSpeed((action.Direction) * (1 + actor.ActorData.AGI/10));
                    actor.movement.MoveTowardTarget(action.Direction + actor.transform.position, 2f, 1 + (float)actor.ActorData.AGI / 10);
                //Show guide and store Direction in skill;
                //guide.transform.position = player.transform.position + GetRelativeToCamera(deltaScaled * referencedAction.StickMult);
                //referencedAction.Direction = GetRelativeToCamera(deltaScaled);


                if (timer >= duration)
                {
                    onMoveComplete.Invoke();
                }
                return;
            }
            else
            {

                // Calculate the squared distance to the destination
                sqrMag = (destination - actor.transform.position).sqrMagnitude;

                // Check if the actor has reached the destination or is moving away
                if (sqrMag <= minDistance * minDistance || sqrMag > lastSqrMag)
                {
                    onMoveComplete.Invoke();
                }

                // Update the last squared magnitude for future comparison
                lastSqrMag = sqrMag;
            }
        }
    }
}