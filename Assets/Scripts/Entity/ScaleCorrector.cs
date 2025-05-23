using UnityEngine;

public class ScaleCorrector : MonoBehaviour
{
    private Transform m_TopParentTransform;
    private void Awake()
    {
        Transform t = transform;
        while (t.parent != null)
        {
            t = t.parent;
        }
        m_TopParentTransform = t;
    }

    private void LateUpdate()
    {
        if (Mathf.Sign(m_TopParentTransform.localScale.x) != Mathf.Sign(transform.localScale.x))
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
}
