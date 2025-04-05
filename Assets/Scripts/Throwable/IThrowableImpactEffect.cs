using UnityEngine;

public interface IThrowableImpactEffect
{
    void ApplyImpactEffect(GameObject victim, Vector2 point, Vector2 normal);
}
