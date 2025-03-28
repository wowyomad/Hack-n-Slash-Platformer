using System;
using Unity.VisualScripting;
using UnityEngine;

public interface IDestroyable
{
    public event Action<IDestroyable> OnDestroyed;
}


[RequireComponent(typeof(CharacterController2D))]
public class Enemy : MonoBehaviour, IHittable, IDamageable, IDestroyable
{
    [HideInInspector] public CharacterController2D Controller;
    public StateMachine StateMachine;
    public Vector3 Velocity;

    public event Action<float, Vector2> OnTakeDamage;
    public event Action OnHit;
    public event Action<IDestroyable> OnDestroyed;

    private void OnEnable()
    {
        StateMachine = new StateMachine();
        StateMachine.ChangeState(new StandartEnemyIdleState(this));
    }

    
    private void Awake()
    {
        Controller = GetComponent<CharacterController2D>();
    }
    private void Update()
    {
        StateMachine.Update();
    }

    public void TakeDamage(float value, Vector2 normal)
    {
        Debug.Log($"Tryinng to invoke OnTakeDamage with: {value}, {normal}");
        OnTakeDamage?.Invoke(value, normal);
    }
    public void TakeHit()
    {
        Debug.Log($"Tryinng to invoke OnTakeHit");
        OnHit?.Invoke();
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }
}
