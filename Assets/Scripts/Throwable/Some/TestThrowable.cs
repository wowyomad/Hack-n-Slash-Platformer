using TheGame;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ThrowableKnife : MonoBehaviour, IThrowableWithThrower
{
    [Header("Configuration")]
    [SerializeField] private float m_Speed = 20f;
    [SerializeField] private Sprite m_IconSprite;
    [SerializeField] private float m_DestroyDelayOnImpact = 0.1f;

    private Rigidbody2D m_RB;
    private GameObject m_Thrower;
    private SpriteRenderer m_SpriteRenderer;
    private bool m_Thrown = false;

    public Sprite Icon => m_IconSprite;

    public event IThrowable.ImpactEvent Impact;
    public event IThrowable.ThrownEvent Thrown;

    private void Awake()
    {
        m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        m_SpriteRenderer.enabled = false;
        m_RB = GetComponent<Rigidbody2D>();
        if (m_RB == null)
        {
            Debug.LogError("TestThrowable requires a Rigidbody2D component.", this);
        }
        m_RB.bodyType = RigidbodyType2D.Kinematic;

    }

    public void SetThrower(GameObject thrower)
    {
        m_Thrower = thrower;
    }

    public void ThrowInDirection(Vector2 origin, Vector2 direction)
    {
        if (m_Thrown)
        {
            Debug.LogWarning("TestThrowable has already been thrown.", this);
            return;
        }
        m_SpriteRenderer.enabled = true;

        transform.position = origin;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);

        m_RB.bodyType = RigidbodyType2D.Dynamic;
        m_RB.linearVelocity = direction.normalized * m_Speed;

        m_Thrown = true;
        Thrown?.Invoke(direction.normalized);
        Debug.Log($"TestThrowable thrown from {origin} in direction {direction.normalized} by {m_Thrower?.name ?? "Unknown"}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!m_Thrown) return;

        if (m_Thrower != null && other.gameObject == m_Thrower)
        {
            return;
        }

        GameObject victim = other.gameObject;

        Debug.Log($"TestThrowable impacted with {victim.name} at {transform.position}");

        if (victim.TryGetComponent<IHittable>(out var hittable))
        {
            HitData hitData = new HitData(m_Thrower);
            hittable.TakeHit(hitData);
        }

        Impact?.Invoke(victim, transform.position, Vector2.up);

        Destroy(gameObject, m_DestroyDelayOnImpact);
        m_Thrown = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!m_Thrown) return;

        GameObject victim = collision.gameObject;



        if (m_Thrower != null && victim == m_Thrower)
        {
            return;
        }

        ContactPoint2D contact = collision.contacts[0];

        Debug.Log($"TestThrowable impacted with {victim.name} at {contact.point}");

        if (victim.TryGetComponent<IHittable>(out var hittable))
        {
            HitData hitData = new HitData(m_Thrower);
            hittable.TakeHit(hitData);
        }

        Impact?.Invoke(victim, contact.point, contact.normal);

        Destroy(gameObject, m_DestroyDelayOnImpact);
        m_Thrown = false;
    }
}