using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Utility
{
    /// <summary>
    /// Controls the post-processing volume weight based on timeScale.
    /// Configure your slowdown visual effect in the Volume profile, 
    /// and this script will fade it in/out proportional to slowdown intensity.
    /// </summary>
    public class SlowdownVisualEffect : MonoBehaviour
    {
        private Volume volume;
        private float targetWeight = 0f;
        private float currentWeight = 0f;
        
        [SerializeField] private float smoothSpeed = 5f; // Higher = faster smoothing
        [SerializeField] private bool debugMode = false;

        private void OnEnable()
        {
            volume = GetComponent<Volume>();
            if (volume == null)
            {
                Debug.LogError("SlowdownVisualEffect: No Volume component found on this GameObject.");
            }
            else
            {
                currentWeight = volume.weight;
            }
        }

        private void Update()
        {
            if (volume == null) return;

            // Calculate target weight based on timeScale
            // At timeScale = 1 (normal speed): intensity = 0 (volume weight = 0)
            // At timeScale = 0 (completely frozen): intensity = 1 (volume weight = 1)
            targetWeight = 1f - Mathf.Clamp01(Time.timeScale);

            // Smoothly interpolate toward target weight
            currentWeight = Mathf.Lerp(currentWeight, targetWeight, Time.unscaledDeltaTime * smoothSpeed);
            volume.weight = currentWeight;

            if (debugMode)
                Debug.Log($"TimeScale: {Time.timeScale:F2}, Target: {targetWeight:F2}, Current Weight: {volume.weight:F2}");
        }

        /// <summary>
        /// Reset the volume weight to 0 gradually (useful for battles starting/ending)
        /// </summary>
        public void ResetEffect()
        {
            targetWeight = 0f;
            // currentWeight will smoothly lerp to 0 over time
        }
    }
}
