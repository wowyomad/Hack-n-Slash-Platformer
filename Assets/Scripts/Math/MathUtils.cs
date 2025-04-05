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
}