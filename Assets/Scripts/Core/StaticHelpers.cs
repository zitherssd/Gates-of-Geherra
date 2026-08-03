using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public static class StaticHelpers
    {
        public static float LinearMap(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((input - inputMin) / (inputMax - inputMin));
        }

        public static float ApplyEasing(float t, LeanTweenType easeType)
        {
            switch (easeType)
            {
                case LeanTweenType.easeInQuad:
                    return t * t;

                case LeanTweenType.easeOutQuad:
                    return t * (2 - t);

                case LeanTweenType.easeInOutQuad:
                    return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;

                case LeanTweenType.easeInCubic:
                    return t * t * t;

                case LeanTweenType.easeOutCubic:
                    return (--t) * t * t + 1;

                case LeanTweenType.easeInOutCubic:
                    return t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

                case LeanTweenType.easeInQuart:
                    return t * t * t * t;

                case LeanTweenType.easeOutQuart:
                    return 1 - (--t) * t * t * t;

                case LeanTweenType.easeInOutQuart:
                    return t < 0.5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;

                case LeanTweenType.easeInQuint:
                    return t * t * t * t * t;

                case LeanTweenType.easeOutQuint:
                    return 1 + (--t) * t * t * t * t;

                case LeanTweenType.easeInOutQuint:
                    return t < 0.5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;

                case LeanTweenType.easeInSine:
                    return 1 - Mathf.Cos((t * Mathf.PI) / 2);

                case LeanTweenType.easeOutSine:
                    return Mathf.Sin((t * Mathf.PI) / 2);

                case LeanTweenType.easeInOutSine:
                    return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;

                case LeanTweenType.easeInExpo:
                    return t == 0 ? 0 : Mathf.Pow(2, 10 * (t - 1));

                case LeanTweenType.easeOutExpo:
                    return t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);

                case LeanTweenType.easeInOutExpo:
                    if (t == 0) return 0;
                    if (t == 1) return 1;
                    return t < 0.5f ? Mathf.Pow(2, 10 * (2 * t - 1)) / 2 : (2 - Mathf.Pow(2, -10 * (2 * t - 1))) / 2;

                case LeanTweenType.easeInCirc:
                    return 1 - Mathf.Sqrt(1 - t * t);

                case LeanTweenType.easeOutCirc:
                    return Mathf.Sqrt(1 - (--t) * t);

                case LeanTweenType.easeInOutCirc:
                    return t < 0.5f ? (1 - Mathf.Sqrt(1 - 4 * t * t)) / 2 : (Mathf.Sqrt(1 - (2 * t - 2) * (2 * t - 2)) + 1) / 2;

                case LeanTweenType.linear:
                    return t;

                case LeanTweenType.easeSpring:
                    return Mathf.Sin(t * Mathf.PI * (0.2f + 2.5f * t * t * t)) * Mathf.Pow(1 - t, 2.2f) + t;

                case LeanTweenType.easeInBounce:
                    return 1 - EaseOutBounce(1 - t);

                case LeanTweenType.easeOutBounce:
                    return EaseOutBounce(t);

                case LeanTweenType.easeInOutBounce:
                    return t < 0.5f ? (1 - EaseOutBounce(1 - 2 * t)) / 2 : (1 + EaseOutBounce(2 * t - 1)) / 2;

                case LeanTweenType.easeInBack:
                    float c1 = 1.70158f;
                    float c3 = c1 + 1;
                    return c3 * t * t * t - c1 * t * t;

                case LeanTweenType.easeOutBack:
                    c1 = 1.70158f;
                    c3 = c1 + 1;
                    return 1 + c3 * (--t) * t * t + c1 * t * t;

                case LeanTweenType.easeInOutBack:
                    c1 = 1.70158f;
                    c3 = c1 * 1.525f;
                    return t < 0.5f
                        ? (Mathf.Pow(2 * t, 2) * ((c3 + 1) * 2 * t - c3)) / 2
                        : (Mathf.Pow(2 * t - 2, 2) * ((c3 + 1) * (t * 2 - 2) + c3) + 2) / 2;

                // You can add more LeanTween types here if necessary...

                default:
                    return t; // Default is linear easing
            }
        }
        private static float EaseOutBounce(float t)
        {
            if (t < 1 / 2.75f)
            {
                return 7.5625f * t * t;
            }
            else if (t < 2 / 2.75f)
            {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            else if (t < 2.5 / 2.75f)
            {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / 2.75f;
                return 7.5625f * t * t + 0.984375f;
            }
        }
    }
}