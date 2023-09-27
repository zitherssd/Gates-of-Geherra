using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class DamagePopup : MonoBehaviour
    {
        private TextMeshPro damageText;
        public float floatSpeed = 1.0f;
        public float fadeDuration = 1.0f;

        private float fadeTimer = 0f;

        public void Initialize(float damageAmount)
        {
            damageText = GetComponentInChildren<TextMeshPro>();
            damageText.text = damageAmount.ToString("F1");
            damageText.fontSize = LinearMap(damageAmount, 0, 30, 1.5f, 6);

            Destroy(gameObject, fadeDuration);
        }

        private void Update()
        {
            // Update the fade timer
            fadeTimer += Time.deltaTime;

            // Calculate the alpha based on the fade timer and duration
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);

            // Update the text color with the calculated alpha
            Color textColor = damageText.color;
            damageText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);

            // Move the text upwards
            transform.Translate(Vector3.up * floatSpeed * Time.deltaTime, Space.World);

            // Destroy the game object if it has faded out
            if (fadeTimer >= fadeDuration)
            {
                Destroy(gameObject);
            }
        }

        float LinearMap(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((input - inputMin) / (inputMax - inputMin));
        }
    }
}