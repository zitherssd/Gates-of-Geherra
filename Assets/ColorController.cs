using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorController : MonoBehaviour
{
    public Color mainColor = Color.white;
    public Color secondaryColor = Color.black;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        propBlock.SetTexture("_MainTex", GetComponent<SpriteRenderer>().sprite.texture);

        // Set mainColor property
        propBlock.SetColor("_MainColor", mainColor);

        // Set secondaryColor property
        propBlock.SetColor("_SecondaryColor", secondaryColor);

        // Apply the property block to the renderer
        renderer.SetPropertyBlock(propBlock);
    }
}
