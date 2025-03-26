using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SomeThrowable : MonoBehaviour, IThrowable
{
    private Rigidbody2D m_RigidBody;

    public event Action<GameObject> OnImpact;
    [SerializeField] private float m_Velocity = 1.0f;
    [SerializeField] private float m_MaxThrowDistanceBeforeImpact = 10f;

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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnImpact?.Invoke(collision.gameObject);
        Reset();
    }

    private void Reset()
    {
        m_RigidBody.linearVelocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}
