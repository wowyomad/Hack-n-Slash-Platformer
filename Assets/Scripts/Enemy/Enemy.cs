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
    public CharacterMovementStats Movement;
    public StateMachine<IEnemyState> StateMachine;
    public Vector3 Velocity;

    public GameObject PlayerReference;

    public bool CanTakeHit => StateMachine.CurrentState is IEnemyVulnarableState;

    public event Action<float, Vector2> OnTakeDamage;
    public event Action OnHit;
    public event Action<IDestroyable> OnDestroyed;

    private void OnEnable()
    {
        StateMachine = new StateMachine<IEnemyState>();
        StateMachine.ChangeState(new StandartEnemyIdleState(this));
    }

    
    private void Awake()
    {
        Controller = GetComponent<CharacterController2D>();
        PlayerReference = GameObject.FindWithTag("Player");
    }
    private void Update()
    {
        StateMachine.Update();
        Controller.Move(Velocity * Time.deltaTime);
    }

    public void TakeDamage(float value, Vector2 normal)
    {
        OnTakeDamage?.Invoke(value, normal);
    }
    public void TakeHit()
    {
        if (CanTakeHit)
        {
            OnHit?.Invoke();
            EventBus<EnemyHitEvent>.Raise(new EnemyHitEvent { EnemyPosition = transform.position });
        }
    }

    public void ApplyGravity()
    {
        Velocity.y += Movement.Gravity * Time.deltaTime;
        Velocity.y = Mathf.Max(Velocity.y, Velocity.y, Movement.MaxGravityVelocity);
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }
}
