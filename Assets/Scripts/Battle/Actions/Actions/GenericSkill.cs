using Assets.Scripts.Battle.Actions.Actions.Effects;
using Assets.Scripts.Battle.Actions.HitWindows;
using Assets.Scripts.Battle.Actor.States;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "AnimationAction", menuName = "ScriptableObjects/Action/Generic")]
    public class GenericSkill : BaseSkill
    {
        public AnimationPhase animationPhase;
        
        [SerializeReference, SubclassSelector]
        public List<IEffect> OnStartEffects = new List<IEffect>();

        [SerializeReference, SubclassSelector]
        public List<IEndableEffect> OnEndEffects = new List<IEndableEffect>();
        
        [SerializeReference, SubclassSelector]
        public List<IUpdateableEffect> OnUpdateEffects = new List<IUpdateableEffect>();

        private List<IEndableEffect> runtimeEndEffects;
        private List<IUpdateableEffect> runtimeUpdateEffects;
        private float actionElapsedTime = 0f;
        private float animatedFrameTime = 0f;
        private int currentFrame = 0;
        private HitWindowManager hitWindowManager;
        private int currentActiveWindowIndex = -1;
        private Animator animator;
        
        public int CurrentActiveWindowIndex => currentActiveWindowIndex;
        
        public HitWindowManager HitWindowMgr => hitWindowManager;
        
        protected override void PerformSpecific(Actor.Actor casterActor, Action onPerformEnd)
        {
            actionElapsedTime = 0f;
            animatedFrameTime = 0f;
            currentFrame = 0;
            animator = casterActor.GetAnimator();
            hitWindowManager = new HitWindowManager(animationPhase.hitWindows);
            runtimeEndEffects = new List<IEndableEffect>(OnEndEffects);
            runtimeUpdateEffects = new List<IUpdateableEffect>(OnUpdateEffects);
            
            // Sync animation from animationPhase
            Animation = animationPhase.animation;

            foreach (var effect in OnStartEffects)
            {
                if (effect is IEndableEffect endable) runtimeEndEffects.Add(endable);
                if (effect is IUpdateableEffect updateable) runtimeUpdateEffects.Add(updateable);
                effect.Eval(_caster, this);
            }
            casterActor.state.TransitionTo<ActingState>().Set(this);
        }

        protected override void Cleanup(ActionEndReason reason)
        {
            base.Cleanup(reason);
            foreach (var effect in runtimeEndEffects)
            {
                effect.End(_caster, this);
            }
        }

        public override void OnUpdate(float dt)
        {
            actionElapsedTime += dt;
            int previousFrame = currentFrame;
            
            // Accumulate frame time based on current animator speed
            // This prevents frame skipping when animator speed changes mid-action
            float animatorSpeed = animator != null ? animator.speed : 1f;
            animatedFrameTime += dt * animatorSpeed * 60f;
            currentFrame = Mathf.FloorToInt(animatedFrameTime);
            
            // Determine which windows are active this frame and run their effects
            bool anyWindowActive = false;
            for (int i = 0; i < animationPhase.hitWindows.Count; i++)
            {
                var window = animationPhase.hitWindows[i];
                bool isActive = currentFrame >= window.startFrame && currentFrame <= window.endFrame;
                bool skippedOverWindow = previousFrame < window.startFrame && currentFrame > window.endFrame;
                bool shouldTriggerEffects = isActive || skippedOverWindow;
                
                
                if (shouldTriggerEffects)
                {
                    currentActiveWindowIndex = i;
                    if (isActive)
                    {
                        anyWindowActive = true;
                    }

                    // Optional per-player trigger limit for this window.
                    if (!hitWindowManager.CanTriggerWindowForPlayer(_caster, i))
                    {
                        continue;
                    }
                    
                    // Run all window effects
                    if (window.windowEffects != null)
                    {
                        foreach (var effect in window.windowEffects)
                        {
                            if (effect is IEndableEffect endable && !runtimeEndEffects.Contains(endable))
                                runtimeEndEffects.Add(endable);

                            if (effect is IUpdateableEffect updateable && !runtimeUpdateEffects.Contains(updateable))
                                runtimeUpdateEffects.Add(updateable);
                            
                            effect.Eval(_caster, this);
                        }
                    }

                    hitWindowManager.RecordWindowTriggerForPlayer(_caster, i);
                }
            }
            
            if (!anyWindowActive)
            {
                currentActiveWindowIndex = -1;
            }
            
            // Run other updateable effects
            foreach (var effect in runtimeUpdateEffects)
            {
                effect.Update(_caster, this, dt);
            }
        }
        public override void OnEnterWindup(Animator animator)
        {
            animator.speed = animationPhase.windupTimeMult;
        }
        public override void OnEnterRecovery(Animator animator)
        {
            animator.speed = animationPhase.recoveryTimeMult;
        }
    }
}

