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

        private Animator animator;
        private bool ended;

        private Action<Animator> windupHandler;
        private Action<Animator> recoveryHandler;
        private Action hitHandler;
        private Action endHandler;
        public BaseAction action;
        private Actor owner;

        private float windupStartTime;
        private float windupDuration;
        private bool isWindingUp;


        public ActingState(Actor owner)
        {
            this.owner = owner;
        }

        public ActingState Set(BaseAction action)
        {
            if (animator == null) animator = owner.GetAnimator();
            
            this.action = action;

            if (action is BaseSkill skill)
            {
                this.onEnterWindup = skill.OnEnterWindup;
                this.onEnterRecovery = skill.OnEnterRecovery;
                this.onHit = skill.OnHit;

                if (skill.Animation.ToString() == "Roll")
                {
                    float dotProduct = Vector3.Dot(owner.transform.forward, skill.Direction);
                    if (dotProduct < 0)
                        animator.Play("RollBackwards", -1, 0f);
                    else
                        animator.Play("Roll", -1, 0f);
                }
                else
                    animator.Play(skill.Animation.ToString(), -1, 0f);
            }
            return this;
        }

        public void Enter()
        {
            ended = false;
            isWindingUp = false;
        }

        public void Exit()
        {
            animator.speed = 1f;
            if (action is AttackSkill attackSkill)
            {
                attackSkill.Unsubscribe();
            }

            if (!ended)
            {
                onEnd?.Invoke(); // An interruption is a form of end for the UI
                if (action != null)
                {
                    action.EndAction(ActionEndReason.Interrupted);
                }
            }

            action = null;
            onEnterWindup = null;
            onEnterRecovery = null;
            onHit = null;
            onEnd = null;
            onWindupProgress?.Invoke(0);
            
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
            if (ended) return;
            ended = true;
            onEnd?.Invoke();
            if (action != null)
            {
                action.EndAction(ActionEndReason.Completed);
            }
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
            
            if (action is BaseSkill skill)
            {
                skill.OnUpdate(Time.deltaTime);
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
        }
    }
}