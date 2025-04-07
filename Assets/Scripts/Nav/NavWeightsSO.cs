using UnityEngine;

[CreateAssetMenu(fileName = "NavWeights", menuName = "Nav2D/NavWeights", order = 1)]
public class NavWeights : ScriptableObject
{
    public float SurfaceWeight = 1.0f;
    public float JumpWeight = 10.0f;
    public float FallWeight = 10.0f;
    public float TransparentJumpWeight = 10.0f;
    public float TransparentFallWeight = 5.0f;
    public float SlopeWeight = 1.0f;
    public float SlopeUpMultiplier = 1.25f;
    public float SlopeDownMultiplier = 0.75f;
}
