using UnityEngine;

public class ThrowableHitOnImpact : MonoBehaviour, IThrowableImpactEffect
{
    public void ApplyImpactEffect(GameObject collidedObject, Vector2 position, Vector2 normal)
    {
        IHittable hittable;
        if (collidedObject.TryGetComponent<IHittable>(out hittable))
        {
            hittable.TakeHit();
        }
    }
}