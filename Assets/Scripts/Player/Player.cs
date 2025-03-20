using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class Player : MonoBehaviour
{
    [Header("Components")]
    public CharacterController Controller;
    public StateMachine StateMachine;
    public Animator Animator;
    public InputReader Input;

    public float Gravity = -20.0f;
    public Vector3 Velocity;

    PlayerIdleState IdleState;
    PlayerWalkState WalkState;
    PlayerJumpState JumpState;
    PlayerAirState AirState;

    private void Awake()
    {
        Input = Resources.Load<InputReader>("Input/InputReader");
        Controller = GetComponent<CharacterController>();
        Animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
       
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

        Velocity.y += Gravity * Time.deltaTime;
        Controller.Move(Velocity * Time.deltaTime);

        Controller.Move(new Vector2(Input.HorizontalMovement * 10.0f, 0.0f) * Time.deltaTime);
    }

    void InitializeStates()
    {
        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        JumpState = new PlayerJumpState(this);
        AirState = new PlayerAirState(this);

        StateMachine.AddTransition(IdleState, WalkState, () => Input.HorizontalMovement != 0);
        StateMachine.AddTransition(WalkState, IdleState, () => Input.HorizontalMovement == 0);
        StateMachine.AddTransition(JumpState, IdleState, () => Controller.Collisions.Below);
        StateMachine.SetState(IdleState);
    }
}