using UnityEngine;


[CreateAssetMenu(menuName = "Player Movement")]
public class PlayerMovementStats : ScriptableObject
{
    [Header("Walk")]
    [Range(1.0f, 100.0f)] public float MaxWalkSpeed = 12.5f;
    [Range(0.1f, 100.0f)] public float GroundAcceleration = 7.5f;
    [Range(0.1f, 100.0f)] public float GroundDeceleration = 30.0f;
    [Range(0.1f, 100.0f)] public float AirAcceleration = 5.0f;
    [Range(0.1f, 100.0f)] public float AirDeceleration = 5.0f;


    [Header("Run")]
    [Range(1.0f, 100.0f)] public float MaxRunSpeed;

    [Header("Grounded")]
    public LayerMask GroundLayer;
    public float GroundDetectionRayLength = 0.02f;
    public float HeadDetectionRayLength = 0.02f;
    [Range(0.0f, 1.0f)] public float HeadWidth = 0.75f;
}
