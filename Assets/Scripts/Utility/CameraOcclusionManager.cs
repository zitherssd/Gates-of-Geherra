using Assets.Scripts.Battle.Manager;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CameraOcclusionManager : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Only objects on these layers can occlude. Set this to your 'Ditherable' layer.")]
    public LayerMask occlusionLayers = ~0;

    [Tooltip("Half-width of an actor's silhouette in world units. Sample rays spread this far left/right.")]
    public float actorRadius = 0.5f;

    [Tooltip("Total height of an actor's silhouette in world units. Sample rays spread across this height.")]
    public float actorHeight = 2f;

    [Tooltip("Vertical centre of the actor's silhouette, measured up from its transform position.")]
    public float actorAimHeight = 1f;

    [Tooltip("Sample rays per axis across the silhouette (N x N grid). Higher = smoother coverage, more raycasts.")]
    [Range(1, 9)] public int samplesPerAxis = 3;

    [Header("Fade")]
    [Tooltip("How fast opacity eases toward its target. Higher = snappier.")]
    public float fadeSpeed = 8f;

    [Tooltip("Opacity used when an object fully hides an actor (0 = fully see-through).")]
    [Range(0f, 1f)] public float minOpacity = 0.1f;

    [Tooltip("Float shader property that drives the dither / transparency.")]
    public string opacityProperty = "_Opacity";

    [Header("Debug")]
    [Tooltip("Draw the sight-line casts in the Scene view and log detection + property problems.")]
    public bool debug = false;

    private Camera cam;
    private BattleManager battleManager;
    private MaterialPropertyBlock mpb;
    private int opacityId;

    // Smoothed opacity currently applied to each renderer.
    private readonly Dictionary<Renderer, float> currentOpacity = new Dictionary<Renderer, float>();
    // Lowest opacity requested for each renderer this frame (1 = fully opaque).
    private readonly Dictionary<Renderer, float> targetOpacity = new Dictionary<Renderer, float>();
    // Reused buffer so the sample raycasts don't allocate garbage every frame.
    private readonly RaycastHit[] hitBuffer = new RaycastHit[32];
    // Per-actor tally of how many sample rays each blocker intercepts.
    private readonly Dictionary<Renderer, int> blockCounts = new Dictionary<Renderer, int>();
    // Scratch list so we can iterate while removing finished entries.
    private readonly List<Renderer> trackedScratch = new List<Renderer>();
    // Materials we've already validated, so the property warning fires once each.
    private readonly HashSet<Material> validatedMaterials = new HashSet<Material>();

    void Reset()
    {
        int ditherable = LayerMask.NameToLayer("Ditherable");
        if (ditherable >= 0)
            occlusionLayers = 1 << ditherable;
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        battleManager = BattleManager.instance;
        mpb = new MaterialPropertyBlock();
        opacityId = Shader.PropertyToID(string.IsNullOrEmpty(opacityProperty) ? "_Opacity" : opacityProperty);

        // Guard against the mask deserialising to "Nothing" (which would occlude nothing).
        if (occlusionLayers.value == 0)
            occlusionLayers = ~0;

        // Guard against newly-added fields zero-filling on an existing component.
        if (samplesPerAxis < 1) samplesPerAxis = 3;
        if (actorRadius <= 0f) actorRadius = 0.5f;
        if (actorHeight <= 0f) actorHeight = 2f;
    }

    void Update()
    {
        targetOpacity.Clear();

        if (battleManager != null)
            AccumulateOcclusion();

        ApplyFade();
    }

    // For every actor, fires a grid of sample rays across its silhouette and
    // records, per blocker, the fraction of those rays it intercepts. That
    // fraction is "how much of the actor this object hides" => how much it fades.
    private void AccumulateOcclusion()
    {
        Vector3 origin = cam.transform.position;
        int samples = Mathf.Max(1, samplesPerAxis);
        int totalSamples = samples * samples;

        foreach (var actor in battleManager.PlayerActors.Concat(battleManager.EnemyActors))
        {
            if (actor == null)
                continue;

            Vector3 centre = actor.transform.position + Vector3.up * actorAimHeight;
            Vector3 toCentre = centre - origin;
            float centreDist = toCentre.magnitude;
            if (centreDist < Mathf.Epsilon)
                continue;
            Vector3 viewDir = toCentre / centreDist;

            // Build a basis in the plane facing the camera so samples spread across
            // the actor's width and height, not along the view direction.
            Vector3 right = Vector3.Cross(Vector3.up, viewDir);
            if (right.sqrMagnitude < 1e-4f)
                right = Vector3.right; // looking straight up or down
            right.Normalize();
            Vector3 up = Vector3.Cross(viewDir, right);

            blockCounts.Clear();

            for (int xi = 0; xi < samples; xi++)
            {
                float ox = samples == 1 ? 0f : Mathf.Lerp(-1f, 1f, xi / (float)(samples - 1));
                for (int yi = 0; yi < samples; yi++)
                {
                    float oy = samples == 1 ? 0f : Mathf.Lerp(-1f, 1f, yi / (float)(samples - 1));

                    Vector3 samplePoint = centre
                        + right * (ox * actorRadius)
                        + up * (oy * actorHeight * 0.5f);

                    Vector3 sdir = samplePoint - origin;
                    float sdist = sdir.magnitude;
                    if (sdist < Mathf.Epsilon)
                        continue;
                    sdir /= sdist;

                    int count = Physics.RaycastNonAlloc(
                        origin, sdir, hitBuffer, sdist,
                        occlusionLayers, QueryTriggerInteraction.Ignore);

                    if (debug)
                        Debug.DrawLine(origin, samplePoint, count > 0 ? Color.red : Color.green);

                    for (int i = 0; i < count; i++)
                    {
                        Renderer rend = hitBuffer[i].transform.GetComponent<Renderer>();
                        if (rend == null)
                            continue;

                        blockCounts.TryGetValue(rend, out int c);
                        blockCounts[rend] = c + 1;
                    }
                }
            }

            // Convert this actor's coverage into target opacities, keeping the
            // strongest fade when an object blocks more than one actor.
            foreach (var kvp in blockCounts)
            {
                float coverage = kvp.Value / (float)totalSamples;     // 0..1 of the actor hidden
                float desired = Mathf.Lerp(1f, minOpacity, coverage); // more hidden => more transparent

                if (targetOpacity.TryGetValue(kvp.Key, out float existing))
                    desired = Mathf.Min(existing, desired);
                targetOpacity[kvp.Key] = desired;
            }
        }
    }

    // Eases every tracked renderer toward its target opacity and stops
    // tracking the ones that are fully restored.
    private void ApplyFade()
    {
        // Make sure freshly-occluded renderers are tracked.
        foreach (var kvp in targetOpacity)
        {
            if (!currentOpacity.ContainsKey(kvp.Key))
                currentOpacity[kvp.Key] = 1f;
        }

        trackedScratch.Clear();
        trackedScratch.AddRange(currentOpacity.Keys);

        // Frame-rate independent easing factor.
        float t = 1f - Mathf.Exp(-fadeSpeed * Time.deltaTime);

        foreach (Renderer rend in trackedScratch)
        {
            if (rend == null)
            {
                currentOpacity.Remove(rend);
                continue;
            }

            float target = targetOpacity.TryGetValue(rend, out float v) ? v : 1f;
            float opacity = Mathf.Lerp(currentOpacity[rend], target, t);
            currentOpacity[rend] = opacity;

            ValidateMaterial(rend);

            rend.GetPropertyBlock(mpb);
            mpb.SetFloat(opacityId, opacity);
            rend.SetPropertyBlock(mpb);

            // Fully opaque again and no longer blocked -> snap clean and forget it.
            if (target >= 1f && opacity >= 0.999f)
            {
                mpb.SetFloat(opacityId, 1f);
                rend.SetPropertyBlock(mpb);
                currentOpacity.Remove(rend);
            }
        }
    }

    // Warns once per material if it lacks the opacity property, listing the
    // shader's actual float Reference names so the right one can be set.
    private void ValidateMaterial(Renderer rend)
    {
        Material mat = rend.sharedMaterial;
        if (mat == null || !validatedMaterials.Add(mat))
            return;

        if (mat.HasProperty(opacityId))
        {
            if (debug)
                Debug.Log($"[CameraOcclusion] OK: '{mat.name}' on '{rend.name}' has '{opacityProperty}'.", rend);
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[CameraOcclusion] Material '{mat.name}' on '{rend.name}' has NO float property " +
                      $"'{opacityProperty}'. Set 'Opacity Property' to one of this shader's float References:");

        Shader shader = mat.shader;
        int propCount = shader.GetPropertyCount();
        for (int i = 0; i < propCount; i++)
        {
            ShaderPropertyType type = shader.GetPropertyType(i);
            if (type == ShaderPropertyType.Float || type == ShaderPropertyType.Range)
                sb.AppendLine("    " + shader.GetPropertyName(i));
        }

        Debug.LogWarning(sb.ToString(), rend);
    }
}
