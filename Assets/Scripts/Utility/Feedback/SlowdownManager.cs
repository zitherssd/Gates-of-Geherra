using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class SlowdownManager : MonoBehaviour
    {
        public static SlowdownManager instance;

        [Header("State Slowdown")]
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float stateTargetTimeScale = 0.05f;
        [SerializeField] private float stateTransitionDuration = 0.1f;
        [SerializeField] private float holdTransitionDuration = 0.25f;

        [Header("Temporary Slowdown")]
        [Tooltip("Curve where Y is timeScale over the duration. Should start near 0 and end at 1.")]
        [SerializeField] private AnimationCurve temporarySlowdownCurve = AnimationCurve.EaseInOut(0, 0.05f, 1, 1f);

        public event Action OnStateSlowdownStart;
        public event Action OnStateSlowdownEnd;
        public event Action<float> OnTemporarySlowdownStart;
        public event Action OnTemporarySlowdownEnd;

        public enum SlowdownMode
        {
            Normal,
            StateSlowdown,
            HoldResume,
            TemporarySlowdown
        }

        private const float BaseFixedDeltaTime = 0.02f;

        private SlowdownMode currentMode = SlowdownMode.Normal;
        private Coroutine transitionCoroutine;
        private Coroutine temporaryCoroutine;

        public bool IsStateSlowdownActive => currentMode == SlowdownMode.StateSlowdown || currentMode == SlowdownMode.HoldResume;
        public float CurrentTimeScale => Time.timeScale;
        public SlowdownMode CurrentMode => currentMode;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
        }

        public void EnterStateSlowdown()
        {
            if (currentMode == SlowdownMode.StateSlowdown || currentMode == SlowdownMode.HoldResume)
                return;

            CancelTemporarySlowdown();
            currentMode = SlowdownMode.StateSlowdown;
            OnStateSlowdownStart?.Invoke();
            StartTransition(Time.timeScale, stateTargetTimeScale, stateTransitionDuration, transitionCurve, () => currentMode = SlowdownMode.StateSlowdown);
        }

        public void ExitStateSlowdown()
        {
            if (currentMode == SlowdownMode.Normal)
                return;

            CancelAllTransitions();
            currentMode = SlowdownMode.Normal;
            StartTransition(Time.timeScale, 1f, stateTransitionDuration, transitionCurve, () =>
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = BaseFixedDeltaTime;
                OnStateSlowdownEnd?.Invoke();
            });
        }

        public void TriggerTemporarySlowdown(float duration)
        {
            if (IsStateSlowdownActive)
                return;

            CancelTemporarySlowdown();
            currentMode = SlowdownMode.TemporarySlowdown;
            temporaryCoroutine = StartCoroutine(TemporarySlowdownCoroutine(duration));
            OnTemporarySlowdownStart?.Invoke(duration);
        }

        public void StartHoldResume()
        {
            if (currentMode != SlowdownMode.StateSlowdown)
                return;

            currentMode = SlowdownMode.HoldResume;
            StartTransition(Time.timeScale, 1f, holdTransitionDuration, transitionCurve, null);
        }

        public void EndHoldResume()
        {
            if (currentMode != SlowdownMode.HoldResume)
                return;

            currentMode = SlowdownMode.StateSlowdown;
            StartTransition(Time.timeScale, stateTargetTimeScale, holdTransitionDuration, transitionCurve, null);
        }

        public void ResetImmediate()
        {
            CancelAllTransitions();
            currentMode = SlowdownMode.Normal;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = BaseFixedDeltaTime;
            OnStateSlowdownEnd?.Invoke();
        }

        private void StartTransition(float from, float to, float duration, AnimationCurve curve, Action onComplete)
        {
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);

            transitionCoroutine = StartCoroutine(TransitionCoroutine(from, to, duration, curve, onComplete));
        }

        private IEnumerator TransitionCoroutine(float fromTimeScale, float toTimeScale, float duration, AnimationCurve curve, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = curve.Evaluate(t);
                float timeScaleValue = Mathf.Lerp(fromTimeScale, toTimeScale, eased);

                Time.timeScale = timeScaleValue;
                Time.fixedDeltaTime = timeScaleValue * BaseFixedDeltaTime;

                yield return null;
            }

            Time.timeScale = toTimeScale;
            Time.fixedDeltaTime = toTimeScale * BaseFixedDeltaTime;
            transitionCoroutine = null;
            onComplete?.Invoke();
        }

        private IEnumerator TemporarySlowdownCoroutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && currentMode == SlowdownMode.TemporarySlowdown)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float timeScaleValue = temporarySlowdownCurve.Evaluate(t);

                Time.timeScale = timeScaleValue;
                Time.fixedDeltaTime = timeScaleValue * BaseFixedDeltaTime;

                yield return null;
            }

            if (currentMode == SlowdownMode.TemporarySlowdown)
            {
                currentMode = SlowdownMode.Normal;
                Time.timeScale = 1f;
                Time.fixedDeltaTime = BaseFixedDeltaTime;
                OnTemporarySlowdownEnd?.Invoke();
            }

            temporaryCoroutine = null;
        }

        private void CancelTemporarySlowdown()
        {
            if (temporaryCoroutine != null)
            {
                StopCoroutine(temporaryCoroutine);
                temporaryCoroutine = null;
            }
        }

        private void CancelAllTransitions()
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            CancelTemporarySlowdown();
        }

        [ContextMenu("Reset TimeScale")]
        private void ResetTimeScaleInEditor()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = BaseFixedDeltaTime;
        }
    }
}
