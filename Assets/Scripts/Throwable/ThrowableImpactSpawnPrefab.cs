    using UnityEngine;

public class ThrowableImpactPrefab : MonoBehaviour, IThrowableImpactEffect
{
    public GameObject EffectPrefab;
    [SerializeField] private float m_OffsetDistance = 0.5f;
    public void ApplyImpactEffect(GameObject collidedObject, Vector2 position, Vector2 normal)
    {
        if (EffectPrefab == null)
        {
            return;
        }
        Vector3 offset = -normal * m_OffsetDistance;
        Instantiate(EffectPrefab, transform.position + offset, Quaternion.identity);
    }
}
