using UnityEngine;

public class ThrowableImpactPrefab : MonoBehaviour, IThrowableEffect
{
    public GameObject EffectPrefab; // или че там надо для эффекта

    public void ApplyEffect(GameObject collidedObject)
    {
        if (EffectPrefab != null)
        {
            for (int i = 0; i < 5; i++)
                Instantiate(EffectPrefab, transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0), Quaternion.identity);

        }
    }
}
