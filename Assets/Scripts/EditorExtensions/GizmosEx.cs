using System;
using UnityEngine;

public static class GizmosEx
{
    public static void DrawArrow(Vector3 start, Vector3 end, Color color)
    {
        Color previousColor = Gizmos.color;
        Gizmos.color = color;

        Vector3 direction = (start - end).normalized;
        Vector3 arrowHead = end;
        Gizmos.DrawLine(start, arrowHead);
        Gizmos.DrawLine(arrowHead, arrowHead + Quaternion.Euler(0, 0, 45) * direction * 0.2f);
        Gizmos.DrawLine(arrowHead, arrowHead + Quaternion.Euler(0, 0, -45) * direction * 0.2f);

        Gizmos.color = previousColor;
    }

    public static void DrawCircle(Vector3 position, float radius, Color color, int segments = 36)
    {
        Color previousColor = Gizmos.color;
        Gizmos.color = color;

        float angleStep = 360f / segments;

        Vector3 previousPoint = position + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }

        Gizmos.color = previousColor;
    }

    public static void DrawCone(Vector3 origin, Vector2 direction, float distance, float angle, Color color, int segments = 18)
    {
        Gizmos.color = color;

        Quaternion upRayRotation = Quaternion.AngleAxis(-angle / 2f, Vector3.forward);
        Quaternion downRayRotation = Quaternion.AngleAxis(angle / 2f, Vector3.forward);

        Vector3 upRayDirection = upRayRotation * direction;
        Vector3 downRayDirection = downRayRotation * direction;

        Gizmos.DrawRay(origin, upRayDirection * distance);
        Gizmos.DrawRay(origin, downRayDirection * distance);

        float segmentAngle = angle / segments;
        Vector3 prevPoint = origin + upRayDirection * distance;

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = segmentAngle * i - angle / 2f;
            Quaternion segmentRotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
            Vector3 currentPointDirection = segmentRotation * direction;
            Vector3 currentPoint = origin + currentPointDirection * distance;
            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }
}