using Assets.Scripts.Battle.Components.State.States;
using Assets.Scripts.Pattern;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State
{
    public class ActorStateMachine : StateMachine
    {
        [SerializeField]
        public IdleState idleState;
        public StaggerState staggerState;
        public AirStaggerState airStaggerState;
        public AirNeutralState airNeutralState;
        public LandingState landingState;
        public ActingState actingState;
        public RollState rollState;
        public BlockState blockState;
        public MoveState moveState;
        public GettingUpState gettingUpState;
        private Actor actor;

        public ActorStateMachine(Actor actor)
        {
            this.idleState = new IdleState(actor);
            this.staggerState = new StaggerState(actor);
            this.airStaggerState = new AirStaggerState(actor);
            this.landingState = new LandingState(actor);
            this.airNeutralState = new AirNeutralState(actor);
            this.actingState = new ActingState(actor);
            this.rollState = new RollState(actor);
            this.blockState = new BlockState(actor);
            this.moveState = new MoveState(actor);
            this.gettingUpState = new GettingUpState(actor);
            this.actor = actor;
            Initialize(actingState);
        }


        internal void OnCollisionEnter(Collision collision)
        {
            if (CurrentState != null)
                CurrentState.OnCollisionEnter(collision);
        }



        public void OnEnd()
        {
            if(CurrentState == actingState)
            {
                actingState.OnEnd();
            }
        }

        public void OnHit()
        {
            if(CurrentState == actingState)
            {
                actingState.OnHit();
            }
        }

        internal void EnterWindup()
        {
            if (CurrentState == actingState)
            {
                actingState.EnterWindup(actor.GetAnimator());
            }
        }

        internal void EnterRecovery()
        {
            if (CurrentState == actingState)
            {
                actingState.EnterRecovery(actor.GetAnimator());
            }
        }
    }
}