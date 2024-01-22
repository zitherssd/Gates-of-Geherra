using UnityEngine;

public class VelocityIndicator : MonoBehaviour
{
    public LineRenderer arrowLineRenderer;
    public float maxArrowLength = 2f;
    public float velocityThreshold = 5f;
    public Color startColor = Color.green;
    public Color endColor = Color.green;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Create the LineRenderer component if not assigned in the Inspector
        if (arrowLineRenderer == null)
        {
            arrowLineRenderer = gameObject.AddComponent<LineRenderer>();
            arrowLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        // Always enable the LineRenderer
        arrowLineRenderer.enabled = true;
        arrowLineRenderer.SetWidth(0.1f, 0.01f);
    }

    void LateUpdate()
    {
        UpdateArrow();
    }

    void UpdateArrow()
    {
        Vector3 velocity = rb.velocity;
        float normalizedVelocity = Mathf.Clamp01(velocity.magnitude / velocityThreshold);
        float arrowLength = Mathf.Lerp(0f, maxArrowLength, normalizedVelocity);

        // Calculate the arrow end position based on the magnitude of the velocity
        Vector3 arrowEnd = transform.position + velocity.normalized * arrowLength;

        // Update the LineRenderer to draw the arrow with startColor and endColor
        arrowLineRenderer.positionCount = 2;
        arrowLineRenderer.SetPosition(0, transform.position);
        arrowLineRenderer.SetPosition(1, arrowEnd);
        arrowLineRenderer.startColor = startColor;
        arrowLineRenderer.endColor = endColor;
    }
}
