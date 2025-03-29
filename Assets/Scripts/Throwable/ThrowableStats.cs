using UnityEngine;

[CreateAssetMenu(fileName = "ThrowableStats", menuName = "Throwable/Stats")]
public class ThrowableStats : ScriptableObject
{
    public float Velocity = 10.0f;
    public int MaximumHits = 1;
    public bool CanHitPlayer = false;
    public bool CanHitEnemy = true;
    public bool FalloffOnTarget = false;
    public bool RemoveOnTargetReached = true;
    public float TimeAliveAfterReachedTarget = 3.0f;
    public Vector2 Falloff = Vector2.zero;
    public LayerMask HitLayer;
}
