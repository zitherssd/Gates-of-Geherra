using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Pattern;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor
{
    public class ActorStateMachine : StateMachine
    {
        [SerializeField]
        public IdleState idleState;
        public FumbleState fumbleState;
        public AirStaggerState airStaggerState;
        public AirNeutralState airNeutralState;
        public LandingState landingState;
        public ActingState actingState;
        public RollState rollState;
        public BlockState blockState;
        public MoveState moveState;
        public GettingUpState gettingUpState;
        public StaggerState staggerState;
        public DeathState deathState;
        public bool locked;
        private Actor actor;
        public BaseSkill action;


        public ActorStateMachine(Actor actor)
        {
            this.idleState = new IdleState(actor);
            this.fumbleState = new FumbleState(actor);
            this.airStaggerState = new AirStaggerState(actor);
            this.landingState = new LandingState(actor);
            this.airNeutralState = new AirNeutralState(actor);
            this.actingState = new ActingState(actor);
            this.rollState = new RollState(actor);
            this.blockState = new BlockState(actor);
            this.moveState = new MoveState(actor);
            this.gettingUpState = new GettingUpState(actor);
            this.staggerState = new StaggerState(actor);
            this.deathState = new DeathState(actor);

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
            if (CurrentState == actingState)
            {
                actingState.OnEnd();
            }
        }

        public void OnHit()
        {
            if (CurrentState == actingState)
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

        public bool IsStaggered()
        {
            if (CurrentState == staggerState || CurrentState == fumbleState || CurrentState == airStaggerState)
                return true;
            else return false;
        }
        public bool IsBlocking()
        {
            if (CurrentState == blockState)
                return true;
            else return false;
        }
        internal void EnterRecovery()
        {
            if (CurrentState == actingState)
            {
                actingState.EnterRecovery(actor.GetAnimator());
            }
        }

        public bool IsMoving(out MoveAction moveAction)
        {
            moveAction = null;
            if (CurrentState == moveState)
            {
                moveAction = moveState.action as MoveAction;
                return true;
            }
            else
                return false;
        }
        public bool IsAlive()
        {
            if (CurrentState != deathState)
                return true;
            else
                return false;
        }
        public bool IsIdle()
        {
            if (CurrentState == idleState)
                return true;
            else
                return false;
        }
        public bool IsAttacking(out AttackSkill attackSkill)
        {
            if (CurrentState == actingState && actingState.action is AttackSkill skill)
            {
                attackSkill = skill;
                if (attackSkill.state != AttackSkill.STATE.windup) 
                    return false;
                else
                    return true;
            }
            else
            {
                attackSkill = null;
                return false;
            }
        }
        public void TransitionToIdle()
        {
            TransitionTo(idleState);
        }
    }
}