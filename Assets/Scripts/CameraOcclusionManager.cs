using Assets.Scripts.Battle.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraOcclusionManager : MonoBehaviour
{
    private BattleManager battleManager;
    private Camera camera;
    public float sphereRadius = 1f;
    public float fadeSpeed = 8f;
    public float maxFadeDistance = 5f;

    // Tracks all renderers and their current opacity values
    private Dictionary<Renderer, float> fadeValues = new Dictionary<Renderer, float>();

    // A temporary list to know which objects were occluded this frame
    private HashSet<Renderer> occludedThisFrame = new HashSet<Renderer>();

    private MaterialPropertyBlock mpb;

    void Start()
    {
        battleManager = BattleManager.instance;
        camera = GetComponent<Camera>();
        mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        occludedThisFrame.Clear();

        // Cast a sphere from camera to each actor (players + enemies)
        foreach (var actor in battleManager.PlayerActors.Concat(battleManager.EnemyActors))
        {
            Vector3 origin = camera.transform.position;
            Vector3 dir = actor.transform.position + Vector3.up * 0.5f - origin;
            float distance = dir.magnitude;

            RaycastHit[] hits = Physics.SphereCastAll(origin, sphereRadius, dir.normalized, distance);

            foreach (var hit in hits)
            {
                if (!hit.transform.CompareTag("Level_Occlude"))
                    continue;

                Renderer rend = hit.transform.GetComponent<Renderer>();
                if (rend == null)
                    continue;

                occludedThisFrame.Add(rend);

                // Calculate perpendicular distance from the centerline of the "tube"
                Vector3 toObject = hit.transform.position - origin;
                float distAlongRay = Vector3.Dot(toObject, dir.normalized);
                Vector3 closestPointOnLine = origin + dir.normalized * distAlongRay;
                float distanceFromCenterline = Vector3.Distance(hit.transform.position, closestPointOnLine);

                // Use distance from centerline instead of camera distance
                float targetOpacity = Mathf.Clamp01(distanceFromCenterline / maxFadeDistance);
                targetOpacity = Mathf.Lerp(0.1f, 1f, targetOpacity); // closer = more transparent

                if (!fadeValues.ContainsKey(rend))
                    fadeValues[rend] = 1f;

                fadeValues[rend] = Mathf.Min(fadeValues[rend], targetOpacity);
            }
        }

        UpdateFadeValues();
    }

    private void UpdateFadeValues()
    {
        // Copy keys so we can safely modify dictionary
        List<Renderer> all = new List<Renderer>(fadeValues.Keys);

        foreach (var rend in all)
        {
            bool shouldFade = occludedThisFrame.Contains(rend);

            float currentOpacity = fadeValues[rend];
            float targetOpacity = shouldFade ? currentOpacity : 1f; // fade back in

            // Lerp opacity
            float newOpacity = Mathf.Lerp(currentOpacity, targetOpacity, Time.deltaTime * fadeSpeed);
            fadeValues[rend] = newOpacity;

            // Apply MPB
            rend.GetPropertyBlock(mpb);
            mpb.SetFloat("_Opacity", newOpacity);
            rend.SetPropertyBlock(mpb);

            // If fully restored, stop tracking it
            if (!shouldFade && Mathf.Abs(newOpacity - 1f) < 0.01f)
                fadeValues.Remove(rend);
        }
    }
}
