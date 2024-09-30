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

        public void AnimationHitCallback()
        {
            if(CurrentState == actingState && actingState.Locked == false)
            {
                actingState.onActionHit?.Invoke();
            }
        }

        public void AnimationEndCallback()
        {
            if (actor == BattleManager.instance.PlayerActors[0]) Debug.Log("ANIMATION END CALLBACK");
            if(CurrentState == actingState && actingState.Locked == false)
            {
                actingState.completetest = true;
                actingState.onActionInterrupt = null;
                actingState.onActionComplete?.Invoke();
            }
        }
    }
}