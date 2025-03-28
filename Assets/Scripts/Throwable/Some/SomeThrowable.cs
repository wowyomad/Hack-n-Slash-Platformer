using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ThrowableStats", menuName = "Throwable/Stats")]
public class ThrowableStats : ScriptableObject
{
    public float Velocity = 1.0f;
    public bool CanHitPlayer = false;
    public bool CanHitEnemy = true;
    public bool FalloffOnTarget = false;
    public LayerMask HitLayer;
}


[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class SomeThrowable : MonoBehaviour, IThrowable
{
    private Rigidbody2D m_RigidBody;
    private BoxCollider2D m_Collider;

    public event Action<GameObject, Vector2, Vector2> OnImpact;
    public event Action<Vector2> OnThrow;
    [SerializeField] private ThrowableStats m_Stats;

    private Action m_OnReset => () => GameObject.Destroy(gameObject);


    private void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<BoxCollider2D>();
        m_Collider.isTrigger = true;
        m_RigidBody.linearVelocity = Vector3.zero;
    }

    private void OnEnable()
    {
        SubscribeEffects();

    }

    private void OnDisable()
    {
        UnsubscribeEffects();
    }

    public void Throw(Vector2 origin, Vector2 target)
    {
        gameObject.SetActive(true);
        transform.position = origin;
        var direction = (target - origin).normalized;
        m_RigidBody.linearVelocity = direction * m_Stats.Velocity;

        OnThrow?.Invoke(direction);
    }

    private void FixedUpdate()
    {
        Vector2 displacement = m_RigidBody.linearVelocity * Time.fixedDeltaTime;
        float distanceThisFrame = displacement.magnitude;

        RotateInDirection(m_RigidBody.linearVelocity);
        HandleCollision(distanceThisFrame, displacement);
    }

    private void RotateInDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            m_RigidBody.rotation = angle;
        }
    }

    private void HandleCollision(float distanceThisFrame, Vector2 displacement)
    {
        float angle = m_RigidBody.rotation;
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, m_Collider.size * 0.5f, angle, m_RigidBody.linearVelocity.normalized, distanceThisFrame, m_Stats.HitLayer);
        if (hit.collider != null)
        {
            if (CanHitTarget(hit.collider))
            {
                OnImpact?.Invoke(hit.collider.gameObject, hit.point, hit.normal);
            }
            Reset();
        }
        else if (distanceThisFrame > 0)
        {
            m_RigidBody.MovePosition(m_RigidBody.position + displacement);
        }
    }

    private bool CanHitTarget(Collider2D collider)
    {
        if (collider.GetComponent<Player>() != null && m_Stats.CanHitPlayer)
        {
            return true;
        }
        if (collider.GetComponent<Enemy>() != null && m_Stats.CanHitEnemy)
        {
            return true;
        }
        return false;
    }

    private void Reset()
    {
        m_OnReset?.Invoke();
    }

    private void SubscribeEffects()
    {
        var hitEffects = GetComponents<IThrowableImpactEffect>();
        foreach (var effect in hitEffects)
        {
            OnImpact += effect.ApplyImpactEffect;
        }
        var throwEfects = GetComponents<IThrowableThrowEffect>();
        foreach (var effect in throwEfects)
        {
            OnThrow += effect.ApplyThrowEffect;
        }
    }
    private void UnsubscribeEffects()
    {
        var effects = GetComponents<IThrowableImpactEffect>();
        foreach (var effect in effects)
        {
            OnImpact -= effect.ApplyImpactEffect;
        }
        var throwEfects = GetComponents<IThrowableThrowEffect>();
        foreach (var effect in throwEfects)
        {
            OnThrow -= effect.ApplyThrowEffect;
        }
    }
}