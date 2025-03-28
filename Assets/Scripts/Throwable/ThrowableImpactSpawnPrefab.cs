using UnityEngine;

public class ThrowableImpactPrefab : MonoBehaviour, IThrowableEffect
{
    public GameObject EffectPrefab;
    [SerializeField] private float m_OffsetDistance = 0.5f;
    public void ApplyEffect(GameObject collidedObject, Vector2 collisionPoint)
    {
        if (EffectPrefab == null)
        {
            return;
        }

        Instantiate(EffectPrefab, transform.position, Quaternion.identity);
    }
}
