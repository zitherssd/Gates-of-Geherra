using Assets.Scripts.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Pattern;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State.States
{
    public class ActingState : IState
    {
        public Action<Animator> onEnterWindup;
        public Action<Animator> onEnterRecovery;
        public Action onHit;
        public Action onEnd;
        public Action onInterrupt;

        private Animator animator;
        private bool ended;

        private Action<Animator> windupHandler;
        private Action<Animator> recoveryHandler;
        private Action hitHandler;
        private Action endHandler;
        public BaseSkill action;

        public ActingState(Actor owner)
        {
            this.animator = owner.GetAnimator();
        }

        public ActingState Set(BaseSkill action, Action onEnd)
        {
            this.action = action;
            windupHandler = action.OnEnterWindup;
            recoveryHandler = action.OnEnterRecovery;
            hitHandler = action.OnHit;
            endHandler = onEnd;

            this.onEnterWindup += windupHandler;
            this.onEnterRecovery += recoveryHandler;
            this.onHit += hitHandler;
            this.onEnd += endHandler;
            animator.Play(action.Animation.ToString(), -1, 0f);
            return this;
        }

        public void Enter()
        {
        }

        public void Exit()
        {
            animator.speed = 1f;

            if (windupHandler != null)
                onEnterWindup -= windupHandler;

            if (recoveryHandler != null)
                onEnterRecovery -= recoveryHandler;

            if (hitHandler != null)
                onHit -= hitHandler;

            if (endHandler != null)
                onEnd -= endHandler;

            if (!ended) onInterrupt?.Invoke();
        }

        public void EnterWindup(Animator animator)
        {
            onEnterWindup?.Invoke(animator);
        }
        public void EnterRecovery(Animator animator)
        {
            onEnterRecovery?.Invoke(animator);
        }

        public void OnHit()
        {
            onHit?.Invoke();
        }

        public void OnEnd()
        {
            ended = true;
            onEnd?.Invoke();
        }

        public void Update()
        {
        }

        public void OnCollisionEnter(Collision collision)
        {
        }
    }
}