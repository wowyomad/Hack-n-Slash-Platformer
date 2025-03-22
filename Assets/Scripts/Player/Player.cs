using NUnit.Framework;
using System;
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
    public float m_VelocityXSmoothing = 0.1f;
    public Vector3 Velocity = Vector3.zero;

    private float m_Gravity = 0.0f;
    private float m_JumpVelocity = 0.0f;

    PlayerIdleState IdleState;
    PlayerWalkState WalkState;
    PlayerJumpState JumpState;
    PlayerAirState AirState;

    private bool m_HasJumped;
    private int m_FacingDirection = 1;

    private void Awake()
    {
        Input = Resources.Load<InputReader>("Input/InputReader");
        Controller = GetComponent<CharacterController>();
        Animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        RecalculateGravity();
        m_FacingDirection = transform.localScale.x == 1 ? 1 : -1;
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

        if (m_HasJumped && Controller.Collisions.Below)
        {
            Velocity.y = m_JumpVelocity;
            m_HasJumped = false;
        }

        float targetVelocityX = Input.HorizontalMovement * MoveSpeed;
        if (Controller.Collisions.Right)
        {
            targetVelocityX = Mathf.Min(targetVelocityX, 0);
        }
        else if (Controller.Collisions.Left)
        {
            targetVelocityX = Mathf.Max(targetVelocityX, 0);
        }
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref m_VelocityXSmoothing,
            (Controller.Collisions.Below) ? AccelerationTimeGrounded : AccelerationTimeAirborne);

        Controller.Move(Velocity * Time.deltaTime);
    }

    private void OnJump()
    {
        m_HasJumped = true;
    }

    private void OnJumpCancelled()
    {
        m_HasJumped = false;
    }

    private void OnMove(float direction)
    {
        Flip((int)Mathf.Sign(direction));
    }

    void Flip(int direction)
    {
        if (direction != m_FacingDirection)
        {
            m_FacingDirection = -m_FacingDirection;
            transform.localScale = new Vector3(m_FacingDirection, 1.0f, 1.0f);
        }
    }

    private void RecalculateGravity()
    {
        m_Gravity = -(2 * JumpHeight) / Mathf.Pow(TimeToJumpApex, 2);
        m_JumpVelocity = -m_Gravity * TimeToJumpApex;
    }

    protected void InitializeStates()
    {
        StateMachine = new StateMachine();

        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        JumpState = new PlayerJumpState(this);
        AirState = new PlayerAirState(this);

        At(IdleState, WalkState, () => Input.HorizontalMovement != 0 && Controller.Collisions.Below);
        At(WalkState, IdleState, () => Input.HorizontalMovement == 0 || Mathf.Abs(Velocity.x) <= 0.00001f || !Controller.Collisions.Below);
        At(IdleState, JumpState, () => Velocity.y > 0.0f);
        At(WalkState, JumpState, () => Velocity.y > 0.0f);
        At(JumpState, AirState, () => Velocity.y <= 0.0f);
        At(AirState, IdleState, () => Controller.Collisions.Below);

        StateMachine.SetState(IdleState);
    }


    protected void At(IState from, IState to, Func<bool> condition)
    {
        StateMachine.AddTransition(from, to, condition);
    }
    protected void At(IState from, IState to, IPredicate predicate)
    {
        StateMachine.AddTransition(from, to, predicate);
    }
    protected void Any(IState from, IState to, IPredicate predicate)
    {
        StateMachine.AddAnyTransition(to, predicate);
    }
}