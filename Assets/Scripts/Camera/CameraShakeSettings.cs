using UnityEngine;

public class CameraShakeSettings : ScriptableObject
{
    [Header("Shake Settings")]

    [Range(0.0f, 5.0f)]
    public float ShakeStrength = 0.5f;

    [Range(0.0f, 1.0f)]
    public float ShakeBias = 0.1f;

    [Range(0.0f, 5.0f)]
    public float RandomnessMultiplier = 0.5f;

    [Range(0.0f, 5.0f)]
    public float SmoothTime = 0.1f;

    [Range(0.0f, 2.0f)]
    public float OngoingShakeMultiplier = 0.5f;

    public bool IsScaledOnDistance = true;

    [Range(0.1f, 1000.0f)]
    public float MaxDistance = 20.0f;

    [Range(0.0f, 1.0f)]
    public float MaxDistanceMultiplier = 0.1f;
}