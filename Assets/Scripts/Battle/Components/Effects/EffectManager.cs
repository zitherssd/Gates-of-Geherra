using Assets.Scripts.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.Effects
{
    public class EffectManager
    {
        private readonly Actor owner;
        public LineRenderer lineRenderer;
        private List<Vector3> points;
        private bool drawing;

        public EffectManager(Actor owner)
        {
            this.owner = owner;
            owner.OnDamageApplied += ShowDamagePopup;
            owner.OnPostureApplied += ShowPosturePopup;
            owner.OnDamageApplied += FlashWhite;
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

        private void FlashWhite(float damageAmount)
        {
            var cc = owner.GetComponentInChildren<ColorController>();

            var intensity = (damageAmount / owner.ActorData.maxPosture);
            cc.StartCoroutine(cc.FlashWhite(0.3f, Mathf.Clamp(intensity,0,1.2f)));
        }

        public void SetHitbox(List<Vector3> points)
        {
            this.points = points;
            drawing = true;
        }
        public void Clear()
        {
            drawing=false;
            points = null;
            lineRenderer.positionCount = 0;
        }

        public void UpdatePolygon()
        {
            if(drawing == true)
            {
                if (points == null) return;
                if (points.Count < 2)
                {
                    lineRenderer.positionCount = 0; // Clear the line if there are not enough points
                    return;
                }

                List<Vector3> transformedPoints = new List<Vector3>();
                foreach (var point in points)
                {
                    // Rotate the point relative to the character's forward direction
                    Vector3 rotatedPoint = owner.transform.TransformDirection(point);
                    // Offset the point by the character's position
                    Vector3 worldPoint = owner.transform.position + rotatedPoint;
                    transformedPoints.Add(worldPoint);
                }

                // Close the shape by adding the first point at the end
                if (transformedPoints[0] != transformedPoints[transformedPoints.Count - 1])
                {
                    transformedPoints.Add(transformedPoints[0]);
                }

                // Update the LineRenderer
                lineRenderer.positionCount = transformedPoints.Count;
                lineRenderer.SetPositions(transformedPoints.ToArray());
            }

            if(owner.state.CurrentState != owner.state.actingState)
            {
                Clear();
            }
        }
    }
}