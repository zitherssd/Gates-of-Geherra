using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class CustomLayout : MonoBehaviour
{
    public float gridSpacing = 50f;   // Spacing between grid points
    private PolygonCollider2D polygonCollider;

    void Start()
    {
        polygonCollider = GetComponent<PolygonCollider2D>();
        ArrangeElements();
    }

    void ArrangeElements()
    {
        if (polygonCollider == null) return;

        // Get all child UI elements
        List<RectTransform> uiElements = new List<RectTransform>();
        foreach (Transform child in transform)
        {
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                uiElements.Add(rectTransform);
            }
        }

        if (uiElements.Count == 0) return;

        // Get the bounds of the polygon
        Bounds bounds = polygonCollider.bounds;

        // Create a grid of points within the bounds
        List<Vector2> gridPoints = GenerateGrid(bounds, gridSpacing);

        // Filter points inside the polygon
        List<Vector2> validPoints = FilterPointsInsidePolygon(gridPoints, polygonCollider);

        // Assign UI elements to points
        for (int i = 0; i < uiElements.Count && i < validPoints.Count; i++)
        {
            uiElements[i].anchoredPosition = validPoints[i];
        }
    }

    List<Vector2> GenerateGrid(Bounds bounds, float spacing)
    {
        List<Vector2> gridPoints = new List<Vector2>();
        for (float x = bounds.min.x; x <= bounds.max.x; x += spacing)
        {
            for (float y = bounds.min.y; y <= bounds.max.y; y += spacing)
            {
                gridPoints.Add(new Vector2(x, y));
            }
        }
        return gridPoints;
    }

    List<Vector2> FilterPointsInsidePolygon(List<Vector2> points, PolygonCollider2D polygon)
    {
        List<Vector2> insidePoints = new List<Vector2>();
        foreach (Vector2 point in points)
        {
            if (PointInPolygon(point, polygon))
            {
                insidePoints.Add(point);
            }
        }
        return insidePoints;
    }

    bool PointInPolygon(Vector2 point, PolygonCollider2D polygon)
    {
        int intersections = 0;
        Vector2 origin = new Vector2(float.MinValue, point.y);

        foreach (Vector2[] edge in GetPolygonEdges(polygon))
        {
            if (DoLinesIntersect(origin, point, edge[0], edge[1]))
            {
                intersections++;
            }
        }

        // Odd number of intersections means the point is inside
        return (intersections % 2) != 0;
    }

    List<Vector2[]> GetPolygonEdges(PolygonCollider2D polygon)
    {
        List<Vector2[]> edges = new List<Vector2[]>();
        for (int i = 0; i < polygon.points.Length; i++)
        {
            Vector2 p1 = polygon.transform.TransformPoint(polygon.points[i]);
            Vector2 p2 = polygon.transform.TransformPoint(polygon.points[(i + 1) % polygon.points.Length]);
            edges.Add(new Vector2[] { p1, p2 });
        }
        return edges;
    }

    bool DoLinesIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        // Simple line intersection check
        float det = (p2.x - p1.x) * (q2.y - q1.y) - (p2.y - p1.y) * (q2.x - q1.x);
        if (Mathf.Abs(det) < Mathf.Epsilon) return false;

        float t = ((q1.x - p1.x) * (q2.y - q1.y) - (q1.y - p1.y) * (q2.x - q1.x)) / det;
        float u = ((q1.x - p1.x) * (p2.y - p1.y) - (q1.y - p1.y) * (p2.x - p1.x)) / det;

        return (t >= 0 && t <= 1 && u >= 0 && u <= 1);
    }
}
