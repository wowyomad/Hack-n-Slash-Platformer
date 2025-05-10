using TheGame;
using UnityEngine;

public class ThrowableHitOnImpact : MonoBehaviour, IThrowableImpactEffect
{
    public void ApplyImpactEffect(GameObject victim, Vector2 point, Vector2 normal)
    {
        if (victim.TryGetComponent<IHittable>(out var hittable))
        {
            hittable.TakeHit();
        }
    }
}