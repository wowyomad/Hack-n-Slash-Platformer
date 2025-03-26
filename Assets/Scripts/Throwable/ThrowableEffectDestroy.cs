using UnityEngine;

public class ThrowableDestroyOnImpact : MonoBehaviour, IThrowableEffect
{
    public void ApplyEffect(GameObject collidedObject)
    {
        Destroy(gameObject);
    }
}
