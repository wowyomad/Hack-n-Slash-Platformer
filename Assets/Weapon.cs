using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private Collider2D m_Collider;

    private float m_ColliderWidth;
    private void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        m_Collider.enabled = false;
    }
    public void EnableCollider()
    {
        m_Collider.enabled = true;
    }

    public void DisableCollider()
    {
        m_Collider.enabled = false;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Hit {collision.tag}");
        Enemy enemy;
        if (collision.TryGetComponent<Enemy>(out enemy))
        {
            var direction = (collision.transform.position - transform.position).normalized;
            enemy.TakeDamage(0, direction);
        }
    }

    private void OnTriggerExit(Collider other)
    {
               
    }
}
