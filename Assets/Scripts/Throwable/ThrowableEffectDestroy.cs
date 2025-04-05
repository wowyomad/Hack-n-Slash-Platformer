using UnityEngine;

public class ThrowableDestroyOnImpact : MonoBehaviour, IThrowableImpactEffect
{
    public void ApplyImpactEffect(GameObject victim, Vector2 point, Vector2 normal)
    {
        Destroy(gameObject);
    }
}
