using Assets.Scripts.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.Effects
{
    public class EffectManager
    {
        private readonly Actor.Actor owner;
        public LineRenderer lineRenderer;
        private List<Vector3> points;
        private Vector3 originPoint;
        private bool drawing;
        private bool onPlayer = false;

        public EffectManager(Actor.Actor owner)
        {
            this.owner = owner;
            //owner.DamageApplied += ShowDamagePopup;
            //owner.PostureApplied += ShowPosturePopup;
            //owner.DamageApplied += FlashWhite;
            lineRenderer = owner.GetComponentsInChildren<LineRenderer>().FirstOrDefault();
            
        }

        private void ShowDamagePopup(float damageAmount)
        {
            // Instantiate the damage popup prefab
            GameObject popup = GameObject.Instantiate(EffectsRepository.DamagePopupPrefab, owner.transform.position, Quaternion.identity);

            popup.GetComponentInChildren<DamagePopup>().Initialize(damageAmount);

        }
        private void ShowPosturePopup(float damageAmount)
        {
            // Instantiate the damage popup prefab
            GameObject popup = GameObject.Instantiate(EffectsRepository.PosturePopupPrefab, owner.transform.position, Quaternion.identity);

            // Set the damage amount text
            popup.GetComponentInChildren<DamagePopup>().Initialize(damageAmount);
        }

        public void SetColors()
        {
            var cc = owner.GetComponentInChildren<ColorController>();
            cc.SetColors(owner.ActorData.mainColor, owner.ActorData.secondaryColor);
        }

        public void FlashWhite(float damageAmount)
        {
            var cc = owner.GetComponentInChildren<ColorController>();

            var intensity = (damageAmount / owner.ActorData.maxPosture);
            cc.StartCoroutine(cc.FlashWhite(0.3f, Mathf.Clamp(intensity,0,1.2f)));
        }

        public void SetHitbox(List<Vector3> points)
        {
            this.points = points;
            drawing = true;
            onPlayer = true;
        }

        public void SetHitbox(List<Vector3> points, Vector3 origin)
        {
            this.points = points;
            this.originPoint = origin;
            drawing = true;
            onPlayer = false;
        }
        public void ClearHitbox()
        {
            drawing=false;
            points = null;
            lineRenderer.positionCount = 0;
        }

        public void UpdatePolygon()
        {
            if (!drawing || points == null || points.Count < 2)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            List<Vector3> transformedPoints = new();

            Vector3 hitboxCenter = onPlayer
                ? owner.transform.position                 // Player-centered hitbox
                : originPoint;                             // Offset hitbox center

            foreach (var point in points)
            {
                // Rotate relative to player's rotation
                Vector3 rotatedPoint = owner.transform.TransformDirection(point);

                // Place at the center + rotated local point
                Vector3 worldPoint = hitboxCenter + rotatedPoint;

                transformedPoints.Add(worldPoint);
            }

            // Close polygon
            if (transformedPoints[0] != transformedPoints[^1])
                transformedPoints.Add(transformedPoints[0]);

            lineRenderer.positionCount = transformedPoints.Count;
            lineRenderer.SetPositions(transformedPoints.ToArray());
        }
    }
}