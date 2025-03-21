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


    [Header("Movement")]
    public float JumpHeight = 10.0f;
    public float TimeToJumpApex = 1.0f;
    public float MoveSpeed = 6.0f;
    public float AccelerationTimeAirborne = 0.35f;
    public float AccelerationTimeGrounded = 0.2f;
    public float VelocityXSmoothing = 0.1f;
    public Vector3 Velocity;

    private float m_Gravity;
    private float m_JumpVelocity;

    PlayerIdleState IdleState;
    PlayerWalkState WalkState;
    PlayerJumpState JumpState;
    PlayerAirState AirState;



    private bool m_HasJumped;

    private void Awake()
    {
        Input = Resources.Load<InputReader>("Input/InputReader");
        Controller = GetComponent<CharacterController>();
        Animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
       RecalculateGravity();
    }
    
    private void OnValidate()
    {
        RecalculateGravity();
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

    public void Update()
    {
        StateMachine?.Update();

        if (Controller.Collisions.Above || Controller.Collisions.Below)
        {
            Velocity.y = 0.0f;
        }

        Velocity.y += m_Gravity * Time.deltaTime;

        Debug.Log($"Collisions below: {Controller.Collisions.Below}");

        if (m_HasJumped && Controller.Collisions.Below)
        {
            Velocity.y = m_JumpVelocity;
            m_HasJumped = false;
        }

        float targetVelocityX = Input.HorizontalMovement * MoveSpeed;
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref VelocityXSmoothing,
            (Controller.Collisions.Below) ? AccelerationTimeGrounded : AccelerationTimeAirborne);

        Controller.Move(Velocity * Time.deltaTime);
        Controller.OnUpdate();
    }

    private void OnJump()
    {
        m_HasJumped = true;
    }

    private void OnJumpCancelled()
    {
        m_HasJumped = false;
    }


    private void RecalculateGravity()
    {
        m_Gravity = -(2 * JumpHeight) / Mathf.Pow(TimeToJumpApex, 2);
        m_JumpVelocity = -m_Gravity * TimeToJumpApex;
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