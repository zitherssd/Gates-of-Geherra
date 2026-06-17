using Assets.Scripts.Battle.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraOcclusionManager : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Radius of the sphere cast from the camera toward each actor. Bigger = wider 'view tube'.")]
    public float sphereRadius = 2f;

    [Tooltip("Only objects on these layers can occlude. Set this to your 'Ditherable' layer.")]
    public LayerMask occlusionLayers = 7;

    [Tooltip("Vertical offset added to each actor's position so we aim at the body, not the feet.")]
    public float actorAimHeight = 0.5f;

    [Header("Fade")]
    [Tooltip("How fast opacity eases toward its target. Higher = snappier.")]
    public float fadeSpeed = 8f;

    [Tooltip("Opacity used when an object sits dead-centre on the line of sight (0 = fully see-through).")]
    [Range(0f, 1f)] public float minOpacity = 0f;

    [Tooltip("Float shader property that drives the dither / transparency.")]
    public string opacityProperty = "_Opacity";

    private Camera cam;
    private BattleManager battleManager;
    private MaterialPropertyBlock mpb;
    private int opacityId;

    // Smoothed opacity currently applied to each renderer.
    private readonly Dictionary<Renderer, float> currentOpacity = new Dictionary<Renderer, float>();
    // Lowest opacity requested for each renderer this frame (1 = fully opaque).
    private readonly Dictionary<Renderer, float> targetOpacity = new Dictionary<Renderer, float>();
    // Reused buffer so the sphere casts don't allocate garbage every frame.
    private readonly RaycastHit[] hitBuffer = new RaycastHit[32];
    // Scratch list so we can iterate while removing finished entries.
    private readonly List<Renderer> trackedScratch = new List<Renderer>();

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
    }

    void Update()
    {
        targetOpacity.Clear();

        if (battleManager != null)
            AccumulateOcclusion();

        ApplyFade();
    }

    // Sphere-casts from the camera to every actor and records, per blocker,
    // how strongly it should fade based on how centred it is on the line of sight.
    private void AccumulateOcclusion()
    {
        Vector3 origin = cam.transform.position;

        foreach (var actor in battleManager.PlayerActors.Concat(battleManager.EnemyActors))
        {
            if (actor == null)
                continue;

            Vector3 dir = actor.transform.position + Vector3.up * actorAimHeight - origin;
            float distance = dir.magnitude;
            if (distance < Mathf.Epsilon)
                continue;
            dir /= distance; // normalise

            int count = Physics.SphereCastNonAlloc(
                origin, sphereRadius, dir, hitBuffer, distance,
                occlusionLayers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hitBuffer[i];

                Renderer rend = hit.transform.GetComponent<Renderer>();
                if (rend == null)
                    continue;

                float desired = OpacityForHit(origin, dir, hit);

                // When one object blocks several actors, keep the strongest fade.
                if (targetOpacity.TryGetValue(rend, out float existing))
                    desired = Mathf.Min(existing, desired);
                targetOpacity[rend] = desired;
            }
        }
    }

    // Closer to the centre line of the view => blocks more => more transparent.
    //   perpendicular distance 0           -> minOpacity (fully blocking the actor)
    //   perpendicular distance sphereRadius -> 1 (only grazing the edge of the view)
    private float OpacityForHit(Vector3 origin, Vector3 dir, RaycastHit hit)
    {
        if (hit.distance <= 0f)
            return minOpacity; // Camera is inside / overlapping the collider.

        Vector3 toHit = hit.point - origin;
        float along = Vector3.Dot(toHit, dir);
        Vector3 closestOnLine = origin + dir * along;
        float perpDistance = Vector3.Distance(hit.point, closestOnLine);

        float t = Mathf.Clamp01(perpDistance / Mathf.Max(sphereRadius, Mathf.Epsilon));
        return Mathf.Lerp(minOpacity, 1f, t);
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
        float t = 1f - Mathf.Exp(-fadeSpeed * Time.unscaledDeltaTime);

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
}
