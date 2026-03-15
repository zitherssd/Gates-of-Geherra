using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    /// <summary>
    /// Manages slowdown effects with two distinct modes:
    /// 
    /// 1. STATE-BASED SLOWDOWN: Active until explicitly Reset() called.
    ///    - Used for action execution, windup/recovery phases
    ///    - Immediate timeScale change (no transition)
    ///    - Blocks temporary slowdowns while active
    /// 
    /// 2. TEMPORARY SLOWDOWN: Auto-expires after duration.
    ///    - Used for hit stuns, brief feedback effects
    ///    - Uses animation curves for smooth transitions
    ///    - Only applies if state slowdown is NOT active
    /// </summary>
    public class SlowdownManager : MonoBehaviour
    {
        public static SlowdownManager instance;

        // Serialized animation curves for inspector configuration
        [SerializeField] private AnimationCurve stateSlowdownStartCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve stateSlowdownEndCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float targetSlowdownTimeScale = 0.05f; // Target timeScale during slowdown

        // Events for systems to subscribe to
        public event Action OnStateSlowdownStart;
        public event Action OnStateSlowdownEnd;
        public event Action<float> OnTemporarySlowdownStart; // duration
        public event Action OnTemporarySlowdownEnd;

        private Coroutine activeTemporarySlowdownCoroutine;
        private Coroutine activeStateSlowdownCoroutine;
        
        // State slowdown tracking
        private bool isStateSlowdownActive = false;
        private float stateSlowdownTimeScale = 1f;

        public bool IsStateSlowdownActive => isStateSlowdownActive;
        public float CurrentTimeScale => Time.timeScale;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
        }

        /// <summary>
        /// Activate state-based slowdown with smooth transition through the curve.
        /// Uses the serialized stateSlowdownStartCurve to smoothly reach target timeScale.
        /// Blocks temporary slowdowns while active.
        /// </summary>
        public void SetStateSlowdown()
        {
            // Kill any running temporary slowdown
            if (activeTemporarySlowdownCoroutine != null)
            {
                StopCoroutine(activeTemporarySlowdownCoroutine);
                activeTemporarySlowdownCoroutine = null;
            }

            // Kill any running state slowdown coroutine
            if (activeStateSlowdownCoroutine != null)
            {
                StopCoroutine(activeStateSlowdownCoroutine);
                activeStateSlowdownCoroutine = null;
            }

            isStateSlowdownActive = true;
            OnStateSlowdownStart?.Invoke();
            
            // Animate through the curve smoothly
            activeStateSlowdownCoroutine = StartCoroutine(SetStateSlowdownCoroutine(stateSlowdownStartCurve));
        }

        /// <summary>
        /// Deactivate state-based slowdown and return to normal time.
        /// Uses the serialized stateSlowdownEndCurve for smooth transition back to 1.0
        /// </summary>
        public void ResetStateSlowdown()
        {
            if (!isStateSlowdownActive)
                return;

            isStateSlowdownActive = false;
            
            // Kill any running state slowdown coroutine
            if (activeStateSlowdownCoroutine != null)
            {
                StopCoroutine(activeStateSlowdownCoroutine);
                activeStateSlowdownCoroutine = null;
            }
            
            // Kill any temporary slowdown
            if (activeTemporarySlowdownCoroutine != null)
            {
                StopCoroutine(activeTemporarySlowdownCoroutine);
                activeTemporarySlowdownCoroutine = null;
            }

            StartCoroutine(ResetStateSlowdownCoroutine(stateSlowdownEndCurve));
        }

        /// <summary>
        /// Trigger a temporary slowdown effect that auto-expires after duration.
        /// Ignored if state slowdown is currently active.
        /// </summary>
        public void TriggerTemporarySlowdown(float duration, AnimationCurve curve)
        {
            // State slowdown takes priority - ignore temporary requests
            if (isStateSlowdownActive)
                return;

            // Kill existing temporary slowdown
            if (activeTemporarySlowdownCoroutine != null)
                StopCoroutine(activeTemporarySlowdownCoroutine);

            activeTemporarySlowdownCoroutine = StartCoroutine(TemporarySlowdownCoroutine(duration, curve));
            OnTemporarySlowdownStart?.Invoke(duration);
        }

        /// <summary>
        /// Immediately reset to normal timeScale
        /// </summary>
        public void ResetImmediate()
        {
            isStateSlowdownActive = false;

            if (activeStateSlowdownCoroutine != null)
                StopCoroutine(activeStateSlowdownCoroutine);

            if (activeTemporarySlowdownCoroutine != null)
                StopCoroutine(activeTemporarySlowdownCoroutine);

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            OnStateSlowdownEnd?.Invoke();
        }

        private IEnumerator ResetStateSlowdownCoroutine(AnimationCurve endCurve)
        {
            float duration = 0.1f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                float timeScaleValue = Mathf.Lerp(stateSlowdownTimeScale, 1f, endCurve.Evaluate(t));

                Time.timeScale = timeScaleValue;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;

                yield return null;
            }

            // Ensure clean final state
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            OnStateSlowdownEnd?.Invoke();
        }

        private IEnumerator SetStateSlowdownCoroutine(AnimationCurve startCurve)
        {
            float duration = 0.1f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                // Lerp from 1.0 to targetSlowdownTimeScale, using the curve as easing
                float timeScaleValue = Mathf.Lerp(1f, targetSlowdownTimeScale, startCurve.Evaluate(t));

                Time.timeScale = timeScaleValue;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;

                yield return null;
            }

            // Ensure we reach the target timeScale
            stateSlowdownTimeScale = targetSlowdownTimeScale;
            Time.timeScale = stateSlowdownTimeScale;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
            
            Debug.Log($"State slowdown activated: timeScale = {stateSlowdownTimeScale:F3}");
        }

        private IEnumerator TemporarySlowdownCoroutine(float duration, AnimationCurve curve)
        {
            float elapsedTime = 0f;

            // Slowdown phase
            while (elapsedTime < duration && !isStateSlowdownActive)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                float timeScaleValue = curve.Evaluate(t);

                Time.timeScale = timeScaleValue;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;

                yield return null;
            }

            // If state slowdown was activated during temporary slowdown, exit
            if (isStateSlowdownActive)
                yield break;

            // Return to normal
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            activeTemporarySlowdownCoroutine = null;
            OnTemporarySlowdownEnd?.Invoke();
        }
    }
}
