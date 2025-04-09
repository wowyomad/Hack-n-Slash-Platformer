using UnityEngine;

public static class MathUtils
{
    public static float MapRangeClamped(this float value, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        return Mathf.Clamp(Map(value, inputMin, inputMax, outputMin, outputMax), outputMin, outputMax);
    }
    public static float Map(this float value, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        float t = Mathf.InverseLerp(inputMin, inputMax, value);

        return Mathf.Lerp(outputMin, outputMax, t);
    }

    public static Vector2 ToVector2(this Vector2Int vector)
    {
        return new Vector2(vector.x, vector.y);
    }
    public static Vector2 ToVector2(this Vector3Int vector)
    {
        return new Vector2(vector.x, vector.y);
    }
    public static Vector3 ToVector3(this Vector3Int vector, float z = 0)
    {
        return new Vector3(vector.x, vector.y, z);
    }
    public static Vector3 ToVector3(this Vector2Int vector)
    {
        return new Vector3(vector.x, vector.y);
    }   
}