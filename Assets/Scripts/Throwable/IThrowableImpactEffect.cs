using UnityEngine;

public interface IThrowableImpactEffect
{
    void ApplyImpactEffect(GameObject collidedObject, Vector2 position, Vector2 normal);
}
