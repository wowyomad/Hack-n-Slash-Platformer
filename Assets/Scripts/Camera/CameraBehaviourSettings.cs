using UnityEngine;

[CreateAssetMenu(fileName = "Camera Behaviour Settings", menuName = "Camera/Behaviour Settings")]
public class CameraBehaviourSettings : ScriptableObject
{
    [Header("Following")]
    [Range(0f, 10f)]
    public float FollowSmoothTime = 0.3f;
    public Vector2 DefaultOffset = new Vector3(0.0f, 2.5f);
    public Vector2 SoftZone = new Vector2(1.5f, 0.5f);

    [Header("Target Leading")]
    public Vector2 LeadFactor = new Vector2(0.0f, 0.0f);

    [Range(0f, 5f)]
    public float LeadSmoothTime = 0.2f;

    [Header("Mouse Influence")]
    [Range(0f, 1f)]
    public float CenterRadius = 0.1f;

    [Range(0f, 1f)]
    public float MaxMouseOffsetX = 0.2f;
    [Range(0f, 1f)]
    public float MaxMouseOffsetY = 0.2f;
    [Range(0f, 5f)]
    public float MouseInfluenceSmoothTime = 0.1f;
    public bool ResetMouseOffsetOutsideBounds = false;

    [Header("Zoom")]
    public float MinZoom = 0.5f;
    public float MaxZoom = 2.0f;
    public float ZoomStep = 0.5f;
}
