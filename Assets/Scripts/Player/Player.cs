using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    private float m_Gravity = -10.0f;

    public CharacterController Controller;
    public StateMachine StateMachine;
    public Animator Animator;
    public InputReader Input;

    PlayerIdleState IdleState;
    PlayerWalkState WalkState;
    PlayerJumpState JumpState;
    PlayerAirState AirState;

    private bool m_IsJumping;

    private void Awake()
    {
        Input = Resources.Load<InputReader>("Input/InputReader");
        Controller = GetComponent<CharacterController>();
        Animator = GetComponentInChildren<Animator>();
    }



    private void Start()
    {
       
    }

    private void OnJump()
    {
        m_IsJumping = true;
    }
    private void OnJumpCancelled()
    {
        m_IsJumping = false;
    }

    private void OnEnable()
    {
        Input.ListenEvents(this);

        InitializeStates();
    }

    private void OnDisable()
    {
        Input.StopListening(this);
    }

    private void Update()
    {
        StateMachine?.Update(); 
    }

    void InitializeStates()
    {
        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        JumpState = new PlayerJumpState(this);
        AirState = new PlayerAirState(this);

        StateMachine.AddTransition(IdleState, WalkState, () => Input.Movement != 0);
        StateMachine.AddTransition(WalkState, IdleState, () => Input.Movement == 0);

        StateMachine.SetState(IdleState);
    }
}