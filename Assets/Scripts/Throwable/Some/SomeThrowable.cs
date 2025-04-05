using System;
using System.Collections.Generic;
using UnityEngine;
using static IThrowable;


[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class SomeThrowable : MonoBehaviour, IThrowable
{
    private Rigidbody2D m_RigidBody;
    private BoxCollider2D m_Collider;

    public event ImpactEVent Impact;
    public event ThrownEvent Thrown;

    [SerializeField] private ThrowableStats m_Stats;

    private Action m_OnReset => () => GameObject.Destroy(gameObject);
    private int m_HitCount = 0;
    private HashSet<GameObject> m_HitObjects = new();
    private Vector3 m_TargetPosition;
    public float DistanceToTarget => (m_TargetPosition - transform.position).magnitude;

    private bool m_HasReachedTarget = false;
    private float m_ClosestDistance = float.MaxValue;

    private float m_HasReachedTargetTimer = 0.0f;
    private bool RemoveOnTargetReached => m_Stats.RemoveOnTargetReached && (m_HasReachedTargetTimer >= m_Stats.TimeAliveAfterReachedTarget);


    private void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<BoxCollider2D>();
        m_Collider.isTrigger = true;
        m_RigidBody.linearVelocity = Vector3.zero;
    }

    private void OnValidate()
    {
        
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
        transform.position = origin;
        var direction = (target - origin).normalized;
        m_RigidBody.linearVelocity = direction * m_Stats.Velocity;

        m_TargetPosition = target;
        gameObject.SetActive(true);
        Thrown?.Invoke(direction);
    }

    private void Update()
    {
        if (RemoveOnTargetReached || m_HitCount >= m_Stats.MaximumHits)
        {
            Reset();
        }
        else
        {
            Vector2 displacement = m_RigidBody.linearVelocity * Time.fixedDeltaTime;
            float distanceThisFrame = displacement.magnitude * 0.5f;

            float currentDistance = DistanceToTarget;
            if (currentDistance < m_ClosestDistance)
            {
                m_ClosestDistance = currentDistance;
            }

            if (!m_HasReachedTarget && currentDistance > m_ClosestDistance)
            {
                m_HasReachedTarget = true;
            }

            RotateInDirection(m_RigidBody.linearVelocity);
            HandleCollision(distanceThisFrame, displacement);
        }
        if (m_HasReachedTarget)
        {
            m_HasReachedTargetTimer += Time.deltaTime;
        }
    }

    private void RotateInDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            m_RigidBody.rotation = angle;
        }
    }

    private void HandleCollision(float distance, Vector2 displacement)
    {
        bool hasHitObstacle = false;
        float angle = m_RigidBody.rotation;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, m_Collider.size * 0.1f, 0.0f, m_RigidBody.linearVelocity.normalized, distance, m_Stats.HitLayer);
        foreach (var hit in hits)
        {
            if (hit.collider != null && !m_HitObjects.Contains(hit.collider.gameObject))
            {
                m_HitObjects.Add(hit.collider.gameObject);
                if (CanHitTarget(hit.collider))
                {
                    Impact?.Invoke(hit.collider.gameObject, hit.point, hit.normal);
                    m_HitCount++;
                }
                else
                {
                    IHittable hittable = hit.collider.GetComponent<IHittable>();
                    if (hittable == null)
                    {
                        hasHitObstacle = true;
                    }
                }
            }
        }

        if (hasHitObstacle)
        {
            Reset();
        }

    }

    private bool CanHitTarget(Collider2D collider)
    {
        if (m_HitCount >= m_Stats.MaximumHits)
        {
            return false;
        }

        IHittable hittable;
        if (collider.TryGetComponent(out hittable) && hittable.CanTakeHit)
        {
            if (hittable is Player)
            {
                return m_Stats.CanHitPlayer;
            }
            if (hittable is Enemy)
            {
                return m_Stats.CanHitEnemy;
            }
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
            Impact += effect.ApplyImpactEffect;
        }
        var throwEfects = GetComponents<IThrowableThrownEffect>();
        foreach (var effect in throwEfects) 
        {
            Thrown += effect.ApplyThrowEffect;
        }
    }
    private void UnsubscribeEffects()
    {
        var effects = GetComponents<IThrowableImpactEffect>();
        foreach (var effect in effects)
        {
            Impact -= effect.ApplyImpactEffect;
        }
        var throwEfects = GetComponents<IThrowableThrownEffect>();
        foreach (var effect in throwEfects)
        {
            Thrown -= effect.ApplyThrowEffect;
        }
    }
}