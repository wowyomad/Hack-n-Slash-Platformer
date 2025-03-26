using NUnit.Framework;
using System;
using Unity.VisualScripting;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(CharacterController2D))]
public class Player : MonoBehaviour
{
    [Header("Components")]
    protected StateMachine m_StateMachine;
    public CharacterController2D Controller;
    public Animator Animator;
    public Animation Animation;
    public InputReader Input;

    public Vector3 Velocity = Vector3.zero;
    public CharacterMovementStats Movement;

    public PlayerIdleState IdleState;
    public PlayerWalkState WalkState;
    public PlayerJumpState JumpState;
    public PlayerAirState AirState;

    public Action<IState> OnStateChange;
    public IState CurrentState => m_StateMachine.Current;

    public int FacingDirection { get; private set; }

    private void Awake()
    {
        Input = Resources.Load<InputReader>("Input/InputReader");
        Controller = GetComponent<CharacterController2D>();
        Animator = GetComponentInChildren<Animator>();
        Animation = GetComponentInChildren<Animation>();
    }

    private void Start()
    {
        FacingDirection = transform.localScale.x > 0 ? 1 : -1;
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
        m_StateMachine?.Update();

        Controller.Move(Velocity * Time.deltaTime);
    }

    public void Flip(int direction)
    {
        if (direction != FacingDirection)
        {
            FacingDirection = -FacingDirection;
            transform.localScale = new Vector3(FacingDirection, 1.0f, 1.0f);
        }
    }

    public void Flip(float direction)
    {
        Flip((int)direction);
    }

    protected void InitializeStates()
    {
        m_StateMachine = new StateMachine();

        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        JumpState = new PlayerJumpState(this);
        AirState = new PlayerAirState(this);


        m_StateMachine.ChangeState(IdleState);
    }

    public void ChangeState(IState state)
    {
        m_StateMachine.ChangeState(state);
        OnStateChange.Invoke(state);
    }


    #region likely to be removed
    public void ApplyGravity()
    {
        Velocity.y += Movement.Gravity * Time.deltaTime;
        Velocity.y = Mathf.Max(Velocity.y, Velocity.y, Movement.MaxGravityVelocity);
    }
    protected void At(IState from, IState to, Func<bool> condition)
    {
        m_StateMachine.AddTransition(from, to, condition);
    }
    protected void At(IState from, IState to, IPredicate predicate)
    {
        m_StateMachine.AddTransition(from, to, predicate);
    }
    protected void Any(IState from, IState to, IPredicate predicate)
    {
        m_StateMachine.AddAnyTransition(to, predicate);
    }
    #endregion
}