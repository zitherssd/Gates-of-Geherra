using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class ColorController : MonoBehaviour
    {
        public Color mainColor = Color.white;
        public Color secondaryColor = Color.black;
        private MaterialPropertyBlock propBlock;
        public AnimationCurve curve;

        private SpriteRenderer spriteRenderer;

        void Start()
        {
            propBlock = new MaterialPropertyBlock();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Set the texture from the sprite
            propBlock.SetTexture("_MainTex", spriteRenderer.sprite.texture);

            // Set the mainColor and secondaryColor properties
            propBlock.SetColor("_MainColor", mainColor);
            propBlock.SetColor("_SecondaryColor", secondaryColor);

            // Apply the property block to the renderer
            spriteRenderer.SetPropertyBlock(propBlock);
        }

        public IEnumerator FlashWhite(float duration, float intensity)
        {
            float currentFlashAmount = 0f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                currentFlashAmount = curve.Evaluate(elapsedTime / duration);
                currentFlashAmount *= intensity;
                // Set the flash amount in the property block
                propBlock.SetFloat("_PostureDamageFlashAmount", currentFlashAmount);

                // Apply the property block to the renderer
                spriteRenderer.SetPropertyBlock(propBlock);

                yield return null;
            }
        }
    }
}
