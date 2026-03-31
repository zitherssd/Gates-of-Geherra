using TMPro;
using UnityEngine;

public class FrameRateManager : MonoBehaviour
{
    [Header("Frame Settings")]
    public float TargetFrameRate = 75.0f;
    private TextMeshProUGUI fpsText;

    private float elapsedTime = 0.0f;
    private int frameCount = 0;

    void Awake()
    {
        fpsText = GetComponent<TextMeshProUGUI>();
        Application.targetFrameRate = (int)TargetFrameRate;
    }

    private void Start()
    {
        Application.targetFrameRate = (int)TargetFrameRate;
    }

    void Update()
    {
        // Accumulate frame time and count
        elapsedTime += Time.unscaledDeltaTime;
        frameCount++;

        // Update FPS display every 1 second
        if (elapsedTime >= 1.0f)
        {
            float averageFps = frameCount / elapsedTime;
            fpsText.text = $"{averageFps:0.0}";

            // Reset counters
            elapsedTime = 0.0f;
            frameCount = 0;
        }
    }
}