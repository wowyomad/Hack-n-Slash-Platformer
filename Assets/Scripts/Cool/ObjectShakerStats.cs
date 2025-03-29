using UnityEngine;

[CreateAssetMenu(fileName = "ShakeSettings", menuName = "Cool/Shake Settings", order = 0)]
public class CameraShakerSettings : ScriptableObject
{
    [Range(0.0f, 10.0f)]
    public float ShakeStrength = 0.2f;

    [Range(0.0f, 10.0f)]
    public float SmoothTime = 0.1f;

    [Range(1.0f, 10.0f)]
    public float RandomnessMultiplier = 1.0f;

    [Range(0.0f, 2.0f)]
    public float OngoingShakeMultiplier = 0.5f;

    [Range(0.0f, 1000.0f)]
    public float MaxAffectDistance = 30f;
    public bool UseAffectDistance = true;

    [Range(0.01f, 1.0f)]
    public float ShakeBias = 0.01f;
}