using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State
{
    public class MoveState : IState
    {
        private Battle.Actor actor;
        private Vector3 targetPosition;
        private Action onMoveComplete;
        private float sqrMag;
        private float lastSqrMag;
        private float minDistance = 0.1f;
        private float moveSpeed = 2f;


        public MoveState(Battle.Actor actor, Vector3 targetPosition, Action onMoveComplete)
        {
            this.actor = actor;
            this.targetPosition = targetPosition;
            this.onMoveComplete = onMoveComplete;
        }

        public void Enter()
        {
            Debug.Log(actor.ActorData.name + " entered MoveState");
            actor.audio.PlayAudio("Move2");
            actor.PlayAnimation("Idle");
            lastSqrMag = Mathf.Infinity;
        }

        public void Exit()
        {
            onMoveComplete();
        }

        public void Update()
        {
            sqrMag = (targetPosition - actor.transform.position).sqrMagnitude;

            if (sqrMag <= minDistance * minDistance || sqrMag > lastSqrMag)
            {
                actor.ActorStateMachine.TransitionTo(actor.ActorStateMachine.idleState);
            }
            else
            {
                actor.rigidbody.velocity = (targetPosition - actor.transform.position).normalized * moveSpeed;
            }
            lastSqrMag = sqrMag;
        }
    }
}