using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Core;
using Assets.Scripts.Core;
using Assets.Scripts.Utility;
using UnityEngine;
using Assets.Scripts.Battle.Actions;

namespace Assets.Scripts.Battle.Actor.States
{
    public class MoveState : IState
    {
        private Actor actor;
        private Vector3 destination;
        private float sqrMag;
        private float lastSqrMag;
        private float minDistance = 0.1f;
        private float moveSpeed = 2f;
        private float duration;
        private bool continousAction;
        private float timer;
        public MoveAction action;
        private float deadzoneThreshold = 0.3f;
        private float idleSpeedThreshold = 0.1f;


        public MoveState(Actor actor)
        {
            this.actor = actor;
        }

        public MoveState Set(float duration, MoveAction action)
        {
            this.duration = duration;
            this.action = action;
            continousAction = true;
            return this;
        }

        public MoveState Set(Vector3 destination, MoveAction action)
        {
            this.destination = destination;
            this.action = action;
            continousAction = false;
            return this;
        }

        public void Enter()
        {
            lastSqrMag = Mathf.Infinity;
            //actor.movement.ResetMomentum();
            actor.movement.SetFriction(0);
            timer = 0.2f;

            //if (!continousAction)
            //{
            //    actor.movement.AddForce((destination - actor.transform.position).normalized * (1 + actor.Runtime.Agi / 10));
            //    actor.movement.FaceDirection(destination - actor.transform.position);
            //}
        }

        public void Exit()
        {
            if (action != null && actor.GetCurrentAction() == action)
            {
                action.EndAction(ActionEndReason.Interrupted);
            }
            action = null;
            actor.movement.SetFriction();
            actor.GetAnimator().speed = 1;

        }

        public void OnCollisionEnter(Collision collision)
        {
            if (continousAction && action != null)
            {
                action.EndAction(ActionEndReason.Completed);
            }
        }

        public void Update()
        {
            if (action == null) return;

            if (continousAction)
            {
                timer += Time.deltaTime;
                
                // Check if joystick is in deadzone AND player speed is below threshold
                bool isInDeadzone = action.Direction.magnitude < deadzoneThreshold;
                var animator = actor.GetAnimator();
                animator.speed = actor.movement.speed;
                if (animator.speed > 1) animator.speed = StaticHelpers.LinearMap(animator.speed, 1, 10, 1, 4);
                if (actor.movement.speed < idleSpeedThreshold) actor.PlayAnimation("Idle"); else actor.PlayAnimation("Run");
                
                if (isInDeadzone) actor.movement.SetFriction();
                else actor.movement.SetFriction(0);

                if (timer < 1f)
                {
                    if (!isInDeadzone) actor.movement.MoveInDirection(action.Direction, 2f * timer, 1 + (float)actor.Runtime.Agility / 20);
                }
                else
                    if (!isInDeadzone) actor.movement.MoveInDirection(action.Direction, 2f, 1 + (float)actor.Runtime.Agility / 20);


                if (timer >= duration)
                {
                    action.EndAction(ActionEndReason.Completed);
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
                    action.EndAction(ActionEndReason.Completed);
                }

                // Update the last squared magnitude for future comparison
                lastSqrMag = sqrMag;
            }
        }
    }
}