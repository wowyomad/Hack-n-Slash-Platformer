using System;
using UnityEngine;

public static class DebugEx
{
    public static void DrawArrow(Vector3 start, Vector3 end, Color color, float duration = 5.0f)
    {
        Vector3 direction = (start - end).normalized;
        Vector3 arrowHead = end;
        Debug.DrawLine(start, arrowHead, color, duration);
        Debug.DrawLine(arrowHead, arrowHead + Quaternion.Euler(0, 0, 45) * direction * 0.2f, color, duration);
        Debug.DrawLine(arrowHead, arrowHead + Quaternion.Euler(0, 0, -45) * direction * 0.2f, color, duration);
    }

}