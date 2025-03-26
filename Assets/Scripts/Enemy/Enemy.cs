using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class Enemy : MonoBehaviour, IDamageable
{
    [HideInInspector] public CharacterController2D Controller;
    public StateMachine StateMachine;
    public Vector3 Velocity;

    public Action<float, Vector2> OnTakeDamage { get; set; }

    private void OnEnable()
    {
        StateMachine = new StateMachine();
        StateMachine.ChangeState(new StandartEnemyIdleState(this));
    }

    
    private void Awake()
    {
        Controller = GetComponent<CharacterController2D>();
        StateMachine.ChangeState(new StandartEnemyIdleState(this));
    }
    private void Update()
    {
        StateMachine.Update();
    }
    public void TakeDamage(float value, Vector2 direction)
    {
        Debug.Log($"Damage: {value}, {direction}");
        OnTakeDamage?.Invoke(value, direction);
    }
}
