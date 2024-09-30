using Assets.Scripts.Pattern;
using System;
using System.Collections;
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


        public MoveState(Actor actor)
        {
            this.actor = actor;
        }

        public MoveState Set(Vector3 destination, Action onMoveComplete)
        {
            this.destination = destination;
            this.onMoveComplete = onMoveComplete;
            return this;
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
            onMoveComplete = null;
        }

        public void OnCollisionEnter(Collision collision)
        {
            onMoveComplete.Invoke();
        }

        public void Update()
        {
            sqrMag = (destination - actor.transform.position).sqrMagnitude;

            if (sqrMag <= minDistance * minDistance || sqrMag > lastSqrMag)
            {
                onMoveComplete.Invoke();
            }
            else
            {
                actor.rigidbody.velocity = (destination - actor.transform.position).normalized * moveSpeed;
            }
            lastSqrMag = sqrMag;
        }
    }
}