using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Pattern;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class ActingState : IState, IAttack
    {
        public Action<float> onWindupProgress;
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
        private Actor owner;

        private float windupStartTime;
        private float windupDuration;
        private bool isWindingUp;


        public ActingState(Actor owner)
        {
            this.owner = owner;
        }

        public ActingState Set(BaseSkill action, Action onEnd)
        {
            if (animator == null) animator = owner.GetAnimator();
            this.onEnterWindup = action.OnEnterWindup;
            this.onEnterRecovery = action.OnEnterRecovery;
            this.onHit = action.OnHit;
            this.onEnd = onEnd;

            this.action = action;
            this.onInterrupt = action.OnCancel;

            if (action.Animation.ToString() == "Roll")
            {
                float dotProduct = Vector3.Dot(owner.transform.forward, action.Direction);
                if (dotProduct < 0)
                    animator.Play("RollBackwards", -1, 0f);
                else
                    animator.Play("Roll", -1, 0f);
            }
            else
                animator.Play(action.Animation.ToString(), -1, 0f);
            return this;
        }

        public void Enter()
        {
            isWindingUp = false;
        }

        public void Exit()
        {
            animator.speed = 1f;
            if (action is AttackSkill)
            {
                var asa = action as AttackSkill;
                asa.Unsubscribe();
            }

            onEnterWindup = null;
            onEnterRecovery = null;
            onHit = null;
            onEnd = null;
            onWindupProgress?.Invoke(0);

            if (!ended) onInterrupt?.Invoke();
            owner.effects.ClearHitbox();
        }

        public void EnterWindup(int windupFrames)
        {
            if (windupFrames == 0) isWindingUp = false;
            onEnterWindup?.Invoke(animator);

            windupDuration = windupFrames / 60f;
            windupStartTime = Time.time;
            isWindingUp = true;
        }
        public void EnterRecovery()
        {
            onEnterRecovery?.Invoke(animator);
            isWindingUp = false;
        }

        public void OnHit()
        {
            onHit?.Invoke();
            isWindingUp = false;
        }

        public void OnEnd()
        {
            ended = true;
            onEnd?.Invoke();
        }

        public void Update()
        {
            if (owner.ActorData.isDead())
            {
                owner.movement.AddForce(Vector3.up * 1.5f);
                owner.transform.position += Vector3.up * 0.01f;
                owner.state.TransitionTo<AirStaggerState>();
            }

            if (isWindingUp && windupDuration > 0f)
            {
                float elapsed = (Time.time - windupStartTime) * animator.speed;
                float t = Mathf.Clamp01(elapsed / windupDuration);
                onWindupProgress?.Invoke(t); // fire event with normalized 0–1
            }
            else
                onWindupProgress?.Invoke(0f);
            action.OnUpdate(Time.deltaTime);
        }

        public void OnCollisionEnter(Collision collision)
        {
        }
    }
}