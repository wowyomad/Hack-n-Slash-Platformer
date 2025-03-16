using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField, Header("Movement")]
    protected float MoveSpeed = 5.0f;
    #region Components
    public PlayerStateMachine StateMachine { get; private set; }
    public Animator Animator { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    #endregion

    #region States
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    #endregion

    private void Awake()
    {
        StateMachine = new PlayerStateMachine();

        IdleState = new PlayerIdleState(this, StateMachine, "Idle");  
        MoveState = new PlayerMoveState(this, StateMachine, "Move");
    }
    void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        Rigidbody = GetComponent<Rigidbody2D>();
        StateMachine.Initialize(IdleState);
    }

    void Update()
    {
        StateMachine.CurrentState.Update(); 
    }

    public void SetVelocity(float xVelocity, float yVelocity)
    {
        Rigidbody.linearVelocity = new Vector2(xVelocity * MoveSpeed , yVelocity);
    }
}
