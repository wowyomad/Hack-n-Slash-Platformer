using System;
using UnityEngine;
using TheGame;
using GameActions;

[SelectionBase]
[RequireComponent(typeof(CharacterController2D))]
public class Player : MonoBehaviour, IStateTrackable, IHittable
{
    public enum Trigger
    {
        Idle,
        Walk,
        Jump,
        Air,
        Attack,
        Throw,
    }

    public PlayerIdleState IdleState;
    public PlayerWalkState WalkState;
    public PlayerJumpState JumpState;
    public PlayerAirState AirState;
    public PlayerAttackState AttackState;
    public PlayerThrowState ThrowState;

    [Header("Components")]
    public StateMachine<PlayerBaseState, Trigger> StateMachine;
    public CharacterController2D Controller;
    public Animator Animator;
    public PlayerAnimationEventBehaviour Animation;
    public InputReader Input;

    public Vector3 Velocity => Controller.Velocity;
    public CharacterMovementStatsSO Movement;


    public event Action<IState, IState> StateChanged;
    public event Action Hit;

    public PlayerBaseState CurrentState => StateMachine.State;
    public bool IsGrounded => Controller.IsGrounded;

    public int FacingDirection { get; private set; }
    public Vector3 WorldCursorPosition => Camera.main.ScreenToWorldPoint(Input.CursorPosition);

    public bool CanTakeHit => true; //TODO: 

    private void Awake()
    {
        Input = Resources.Load<InputReader>("Input/InputReader");
        Controller = GetComponent<CharacterController2D>();
        Animator = GetComponentInChildren<Animator>();
        Animation = GetComponentInChildren<PlayerAnimationEventBehaviour>();
    }

    private void Start()
    {
        FacingDirection = transform.localScale.x > 0 ? 1 : -1;

        Controller.ApplyGravity = true;
        Controller.Gravity = Movement.Gravity;
        Controller.MaxGravityVelocity = Movement.MaxGravityVelocity;
    }

    private void OnEnable()
    {
        InitializeStates();

        Input.ListenEvents(this);

        StateMachine.StateChangedEvent += OnStateChanged;
    }

    private void OnDisable()
    {
        Input.StopListening(this);

        StateMachine.StateChangedEvent -= OnStateChanged;
    }

    public void Update()
    {
        StateMachine.Update();
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
        if (direction != 0.0f)
            Flip((int)direction);
    }

    public void TurnToCursor()
    {
        Vector3 mousePosition = Input.CursorPosition;
        Vector3 playerPosition = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 direction = mousePosition - playerPosition;
        Flip(direction.x > 0 ? 1 : -1);
    }

    protected void InitializeStates()
    {
        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        JumpState = new PlayerJumpState(this);
        AirState = new PlayerAirState(this);
        AttackState = new PlayerAttackState(this);
        ThrowState = new PlayerThrowState(this);

        StateMachine = new StateMachine<PlayerBaseState, Trigger>(IdleState);

        StateMachine.Configure(IdleState)
            .Permit(Trigger.Jump, JumpState)
            .Permit(Trigger.Attack, AttackState)
            .Permit(Trigger.Throw, ThrowState)
            .PermitIf(Trigger.Air, AirState, () => !Controller.IsGrounded)
            .PermitIf(Trigger.Walk, WalkState, () =>
                Input.HorizontalMovement > 0.0f && !Controller.IsFacingWallRight ||
                Input.HorizontalMovement < 0.0f && !Controller.IsFacingWallLeft);

        StateMachine.Configure(WalkState)
            .Permit(Trigger.Jump, JumpState)
            .Permit(Trigger.Attack, AttackState)
            .Permit(Trigger.Throw, ThrowState)
            .PermitIf(Trigger.Air, AirState, () => !Controller.IsGrounded)
            .PermitIf(Trigger.Idle, IdleState, () => Input.HorizontalMovement == 0.0f && Controller.IsGrounded);

        StateMachine.Configure(JumpState)
            .Permit(Trigger.Throw, ThrowState)
            .PermitIf(Trigger.Air, AirState, () => Controller.Velocity.y <= 0.0f);

        StateMachine.Configure(AirState)
            .Permit(Trigger.Throw, ThrowState)
            .PermitIf(Trigger.Idle, IdleState, () => Controller.IsGrounded);

        StateMachine.Configure(AttackState)
            .PermitIf(Trigger.Idle, IdleState, () => AttackState.AttackFinished && StateMachine.PreviousState == IdleState)
            .PermitIf(Trigger.Walk, WalkState, () => AttackState.AttackFinished && StateMachine.PreviousState == WalkState)
            .PermitIf(Trigger.Air, AirState, () => AttackState.AttackFinished && StateMachine.PreviousState == AirState);

        StateMachine.Configure(ThrowState)
            .PermitIf(Trigger.Idle, IdleState, () => StateMachine.PreviousState == IdleState)
            .PermitIf(Trigger.Walk, WalkState, () => StateMachine.PreviousState == WalkState)
            .PermitIf(Trigger.Air, AirState, () => StateMachine.PreviousState == AirState && Velocity.y <= 0.0f)
            .PermitIf(Trigger.Jump, JumpState, () =>
                {
                    if (StateMachine.PreviousState == JumpState)
                    {
                        JumpState.Mode = PlayerJumpState.EntryMode.Resume;
                        return true;
                    }
                    return false;
                });
    }

    public void ThrowKnife()
    {
        var knifePrefab = Instantiate(Resources.Load<GameObject>("Prefabs/Throwable/Knife"));
        IThrowable knife;
        if (knifePrefab.TryGetComponent<IThrowable>(out knife))
        {
            Throw(knife);
        }
    }

    public void Throw(IThrowable throwable)
    {
        Vector3 targetPosition = WorldCursorPosition - Velocity;
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        Vector3 origin = transform.position + directionToTarget * 1.0f;
        throwable.Throw(origin, WorldCursorPosition);
    }

    private void OnStateChanged(IState previous, IState next)
    {
        LogStateChange(previous, next);
        StateChanged?.Invoke(previous, next);
    }

    public void TakeHit()
    {
        Hit?.Invoke();
        EventBus<PlayerHitEvent>.Raise(new PlayerHitEvent() { PlayerPosition = transform.position });
    }

    [GameAction(ActionType.Jump)]
    protected void HandleJumpInput()
    {
        if (StateMachine.CanFire(Trigger.Jump))
            StateMachine.Fire(Trigger.Jump);
    }

    [GameAction(ActionType.Attack)]
    protected void HandleAttackInput()
    {
        if (StateMachine.CanFire(Trigger.Attack))
            StateMachine.Fire(Trigger.Attack);
    }

    [GameAction(ActionType.Throw)]
    protected void HandleThrowInput()
    {
        if (StateMachine.CanFire(Trigger.Throw))
            StateMachine.Fire(Trigger.Throw);
    }

    [GameAction(ActionType.Dash)]
    protected void HandleDashInput()
    {
        Controller.PassThrough();
    }

    private void LogStateChange(IState previous, IState next)
    {
        if (previous != null)
            Debug.Log($"Changing state from {previous} to {next}");
        else
            Debug.Log($"Assigning state to {next}");
    }
}