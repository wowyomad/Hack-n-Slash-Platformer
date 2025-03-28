using UnityEngine;

public class ThrowableDestroyOnImpact : MonoBehaviour, IThrowableImpactEffect
{
    public void ApplyImpactEffect(GameObject collidedObject, Vector2 position, Vector2 normal)
    {
        Destroy(gameObject);
    }
}
