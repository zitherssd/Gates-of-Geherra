using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public static class Intersections
    {
        public static bool IsPointInsidePolygon(Vector3 point, List<Vector3> polygon)
        {
            if (polygon == null || polygon.Count < 3)
            {
                Debug.LogWarning("Polygon must have at least 3 points.");
                return false;
            }

            int intersections = 0;

            for (int i = 0; i < polygon.Count; i++)
            {
                Vector3 vertex1 = polygon[i];
                Vector3 vertex2 = polygon[(i + 1) % polygon.Count]; // Wrap around to the first vertex

                // Check if the ray from the point to +X axis intersects the polygon edge
                if (IsIntersecting(point, vertex1, vertex2))
                {
                    intersections++;
                }
            }

            // If the number of intersections is odd, the point is inside the polygon
            return (intersections % 2) == 1;
        }
        public static bool IsIntersecting(Vector3 point, Vector3 vertex1, Vector3 vertex2)
        {
            // Ensure we work in 2D (XZ-plane)
            point.y = 0;
            vertex1.y = 0;
            vertex2.y = 0;

            // Check if the edge straddles the horizontal ray from the point
            if ((vertex1.z > point.z && vertex2.z <= point.z) || (vertex2.z > point.z && vertex1.z <= point.z))
            {
                // Compute the intersection point's X-coordinate
                float t = (point.z - vertex1.z) / (vertex2.z - vertex1.z);
                float intersectionX = vertex1.x + t * (vertex2.x - vertex1.x);

                // Check if the intersection is to the right of the point
                return intersectionX > point.x;
            }

            return false;
        }
        public static bool IsCircleIntersectingLine(Vector3 circleCenter, float radius, Vector3 lineStart, Vector3 lineEnd)
        {
            // Project the circle center onto the line segment and find the closest point
            Vector3 lineDir = lineEnd - lineStart;
            float lineLength = lineDir.magnitude;
            lineDir.Normalize();

            Vector3 pointToCircle = circleCenter - lineStart;
            float t = Mathf.Clamp(Vector3.Dot(pointToCircle, lineDir), 0, lineLength);
            Vector3 closestPoint = lineStart + t * lineDir;

            // Check the distance from the closest point to the circle's center
            float distanceSquared = (closestPoint - circleCenter).sqrMagnitude;
            return distanceSquared <= radius * radius;
        }
    }
}
