using UnityEngine;

public class ThrowableDestroyOnImpact : MonoBehaviour, IThrowableEffect
{
    public void ApplyEffect(GameObject collidedObject, Vector2 position)
    {
        Destroy(gameObject);
    }
}
