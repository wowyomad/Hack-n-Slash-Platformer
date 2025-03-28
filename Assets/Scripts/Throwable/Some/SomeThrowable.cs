using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SomeThrowable : MonoBehaviour, IThrowable
{
    private Rigidbody2D m_RigidBody;

    public event Action<GameObject, Vector2> OnImpact;
    public event Action<Vector2> OnThrow;

    [SerializeField] private float m_Velocity = 1.0f;
    [SerializeField] private bool m_CanHitPlayer = false;

    private void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody2D>();
        Reset();
    }

    private void OnEnable()
    {
        var effects = GetComponents<IThrowableEffect>();
        foreach (var effect in effects)
        {
            OnImpact += effect.ApplyEffect;
        }
    }

    private void OnDisable()
    {
        var effects = GetComponents<IThrowableEffect>();
        foreach (var effect in effects)
        {
            OnImpact -= effect.ApplyEffect;
        }
    }

    public void Throw(Vector2 origin, Vector2 direction)
    {
        gameObject.SetActive(true);
        transform.position = origin;
        m_RigidBody.linearVelocity = direction * m_Velocity;

        OnThrow?.Invoke(direction);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if(!m_CanHitPlayer && collision.gameObject.GetComponent<Player>() != null)
        {
            return;
        }

        OnImpact?.Invoke(collision.gameObject, collision.contacts[0].point);
        Reset();
    }

    private void Reset()
    {
        m_RigidBody.linearVelocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}
