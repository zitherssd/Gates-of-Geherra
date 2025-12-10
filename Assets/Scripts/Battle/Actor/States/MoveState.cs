using Assets.Scripts.Battle.Actions.Actions;
using Assets.Scripts.Pattern;
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
            actor.PlayAnimation("Run");
            lastSqrMag = Mathf.Infinity;
            //actor.movement.ResetMomentum();
            actor.movement.SetFriction(0);
            timer = 0.2f;

            //if (!continousAction)
            //{
            //    actor.movement.AddForce((destination - actor.transform.position).normalized * (1 + actor.ActorData.Agi / 10));
            //    actor.movement.FaceDirection(destination - actor.transform.position);
            //}
        }

        public void Exit()
        {
            if (action != null)
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
                if (timer < 1f)
                {
                    var animator = actor.GetAnimator();
                    //animator.speed = timer * 2 / 3 * (1 + (float)actor.ActorData.AGI/10);
                    animator.speed = actor.movement.speed;
                    if (animator.speed > 1) animator.speed = StaticHelpers.LinearMap(animator.speed, 1, 10, 1, 4);
                    //actor.movement.ChangeSpeed((action.Direction.normalized) * (1 + actor.ActorData.AGI/10) * timer);
                    //actor.movement.MoveTowardTarget(action.Direction + actor.transform.position, 2f * timer, 1 + (float)actor.ActorData.AGI / 10);
                    actor.movement.MoveInDirection(action.Direction, 2f * timer, 1 + (float)actor.ActorData.Agility / 20);

                }
                else
                    //actor.movement.ChangeSpeed((action.Direction) * (1 + actor.ActorData.AGI/10));
                    //actor.movement.MoveTowardTarget(action.Direction + actor.transform.position, 2f, 1 + (float)actor.ActorData.AGI / 10);
                    actor.movement.MoveInDirection(action.Direction, 2f, 1 + (float)actor.ActorData.Agility / 20);
                //Show guide and store Direction in skill;
                //guide.transform.position = player.transform.position + GetRelativeToCamera(deltaScaled * referencedAction.StickMult);
                //referencedAction.Direction = GetRelativeToCamera(deltaScaled);


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