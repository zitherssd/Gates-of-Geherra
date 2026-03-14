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

        // Events for systems to subscribe to
        public event Action OnStateSlowdownStart;
        public event Action OnStateSlowdownEnd;
        public event Action<float> OnTemporarySlowdownStart; // duration
        public event Action OnTemporarySlowdownEnd;

        private Coroutine activeTemporarySlowdownCoroutine;
        
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
        /// Activate state-based slowdown with immediate timeScale change.
        /// Evaluates startCurve to determine target timeScale.
        /// Blocks temporary slowdowns while active.
        /// </summary>
        public void SetStateSlowdown(AnimationCurve startCurve)
        {
            // Kill any running temporary slowdown
            if (activeTemporarySlowdownCoroutine != null)
            {
                StopCoroutine(activeTemporarySlowdownCoroutine);
                activeTemporarySlowdownCoroutine = null;
            }

            // Evaluate curve at end to get final timeScale
            stateSlowdownTimeScale = startCurve.Evaluate(1f);
            
            // Apply immediately
            Time.timeScale = stateSlowdownTimeScale;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;

            isStateSlowdownActive = true;
            OnStateSlowdownStart?.Invoke();

            if (Mathf.Abs(stateSlowdownTimeScale) < 0.01f)
                Debug.Log($"State slowdown activated: timeScale = {stateSlowdownTimeScale:F3} (frozen)");
            else
                Debug.Log($"State slowdown activated: timeScale = {stateSlowdownTimeScale:F3}");
        }

        /// <summary>
        /// Deactivate state-based slowdown and return to normal time.
        /// Uses endCurve for smooth transition back to 1.0
        /// </summary>
        public void ResetStateSlowdown(AnimationCurve endCurve)
        {
            if (!isStateSlowdownActive)
                return;

            isStateSlowdownActive = false;
            
            // Kill any temporary slowdown
            if (activeTemporarySlowdownCoroutine != null)
            {
                StopCoroutine(activeTemporarySlowdownCoroutine);
                activeTemporarySlowdownCoroutine = null;
            }

            StartCoroutine(ResetStateSlowdownCoroutine(endCurve));
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
