using System;
using UnityEngine;
using TheGame;
using GameActions;
using System.Linq;
using Unity.Mathematics;
using System.Collections.Generic;

[SelectionBase]
[RequireComponent(typeof(CharacterController2D))]
public partial class Player : MonoBehaviour, IStateTrackable, IHittable
{

    public enum Trigger
    {
        Idle,
        Walk,
        Jump,
        Air,
        Attack,
        Throw,
        Die,
        Stun,
    }

    #region Events
    public Action PlayerWalkedEvent;
    public Action PlayerIdleEvent;
    public Action PlayerJumpedEvent;
    public Action PlayerInAirEvent;
    public Action PlayerAttackedEvent;
    public Action PlayerThrewEvent;
    public Action PlayerDiedEvent;
    public Action PlayerStunnedEvent;
    #endregion

    #region States
    protected PlayerAnyState AnyState;
    protected PlayerStunnedState StunnedState;
    protected PlayerDeadState DeadState;
    protected PlayerIdleState IdleState;
    protected PlayerWalkState WalkState;
    protected PlayerJumpState JumpState;
    protected PlayerAirState AirState;
    protected PlayerAttackState AttackState;
    protected PlayerThrowState ThrowState;
    #endregion

    [SerializeField] private LayerMask m_EnemyLayerMask;

    [HideInInspector] public StateMachine<PlayerBaseState, Trigger> StateMachine;
    private bool m_Initialized = false;
    
    [HideInInspector] public CharacterController2D Controller;
    [HideInInspector] public PlayerAnimationEvents AnimationEvents;
    [HideInInspector] public InputReader Input;
    protected PlayerBaseState CurrentState => StateMachine.State;
    protected PlayerBaseState PreviousState => StateMachine.PreviousState;

    private PlayerAnimation m_AnimationHandler;

    public Vector3 Velocity => Controller.Velocity;
    public CharacterMovementStatsSO Movement;
    private bool IsVulnarable = true;

    public event Action<IState, IState> StateChanged;
    public event Action Hit;

    public bool IsGrounded => Controller.IsGrounded;

    public int FacingDirection { get; private set; }

    public bool CanTakeHit => !IsVulnarable;
    public float DefaultAttackAnimationDuration = 0.4f;
    private float m_AttackAnimationLength = 0.0f;
    private ActionTimer m_StunnedCooldownTimer;
    private float m_StunCooldownDuration = 1.5f;
    private float m_StunDuration = 0.5f;
    private bool m_CanGetStunned = false;
    private bool IsImmuneToStun = false;

    private List<ActionTimer> m_Timers;

    private void Awake()
    {

        Input = Resources.Load<InputReader>("Input/InputReader");
        Controller = GetComponent<CharacterController2D>();
        AnimationEvents = GetComponentInChildren<PlayerAnimationEvents>();
        m_AnimationHandler = GetComponent<PlayerAnimation>();

        m_StunnedCooldownTimer = new ActionTimer();
        m_StunnedCooldownTimer.SetDuration(m_StunCooldownDuration);
        m_StunnedCooldownTimer.SetFinishedCallback(() => m_CanGetStunned = true);
        m_StunnedCooldownTimer.SetStartedCallback(() => m_CanGetStunned = false);

        m_Timers = new List<ActionTimer>
        {
            m_StunnedCooldownTimer,
        };

        InitializeStates();
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
        Input.ListenEvents(this);

        StateMachine.StateChangedEvent += OnStateChanged;
        StateMachine.StateChangedEvent += InvokeStateChangedEvent;

#if UNITY_EDITOR
        StateMachine.StateChangedEvent += LogStateChange;
#endif
    }

    private void OnDisable()
    {
        Input.StopListening(this);

        StateMachine.StateChangedEvent -= OnStateChanged;
    }

    public void Update()
    {
        StateMachine.Update();
        m_Timers.ForEach(timer => timer.Tick());
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
        if (m_Initialized)
            return;

        if (m_AnimationHandler != null)
        {
            if (m_AnimationHandler.GetAnimationDuration(PlayerAnimation.AttackMeleeAnimationHash, out float attackAnimationDuration) > 0.0f)
            {
                m_AttackAnimationLength = attackAnimationDuration;
            }
            else
            {
                m_AttackAnimationLength = DefaultAttackAnimationDuration;
            }
        }

        AnyState = new PlayerAnyState(this);
        StunnedState = new PlayerStunnedState(this, 0.5f);
        DeadState = new PlayerDeadState(this);
        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        JumpState = new PlayerJumpState(this);
        AirState = new PlayerAirState(this);
        AttackState = new PlayerAttackState(this, m_AttackAnimationLength);
        ThrowState = new PlayerThrowState(this);

        StateMachine = new StateMachine<PlayerBaseState, Trigger>(IdleState);

        StateMachine.Configure(AnyState)
            .IgnoreIf(Trigger.Die, () => !IsVulnarable)
            .Permit(Trigger.Die, DeadState)
            .PermitIf(Trigger.Stun, StunnedState, () => m_CanGetStunned && !StunnedState.IsStunned && !IsImmuneToStun);

        StateMachine.Configure(StunnedState)
            .SubstateOf(AnyState)
            .Permit(Trigger.Idle, IdleState)
            .TriggerIf(Trigger.Idle, () => !StunnedState.IsStunned);

        StateMachine.Configure(IdleState)
            .SubstateOf(AnyState)
            .Permit(Trigger.Jump, JumpState)
            .Permit(Trigger.Attack, AttackState)
            .Permit(Trigger.Throw, ThrowState)
            .Permit(Trigger.Air, AirState)
            .TriggerIf(Trigger.Air, () => !Controller.IsGrounded)
            .Permit(Trigger.Walk, WalkState)
            .TriggerIf(Trigger.Walk, () => Input.HorizontalMovement != 0.0f);

        StateMachine.Configure(WalkState)
            .SubstateOf(AnyState)
            .Permit(Trigger.Jump, JumpState)
            .Permit(Trigger.Attack, AttackState)
            .Permit(Trigger.Throw, ThrowState)
            .Permit(Trigger.Air, AirState)
            .TriggerIf(Trigger.Air, () => !Controller.IsGrounded)
            .Permit(Trigger.Idle, IdleState)
            .TriggerIf(Trigger.Idle, () => Input.HorizontalMovement == 0.0f && Controller.IsGrounded);

        StateMachine.Configure(JumpState)
            .SubstateOf(AnyState)
            .Permit(Trigger.Throw, ThrowState)
            .Permit(Trigger.Air, AirState)
            .TriggerIf(Trigger.Air, () => Controller.Velocity.y <= 0.0f);

        StateMachine.Configure(AirState)
            .SubstateOf(AnyState)
            .Permit(Trigger.Throw, ThrowState)
            .Permit(Trigger.Idle, IdleState)
            .TriggerIf(Trigger.Idle, () => Controller.IsGrounded && Velocity.y == 0.0f);

        StateMachine.Configure(AttackState)
            .SubstateOf(AnyState)
            .Permit(Trigger.Idle, IdleState)
            .Permit(Trigger.Walk, WalkState)
            .Permit(Trigger.Air, AirState)
            .TriggerIf(Trigger.Idle, () => AttackState.AttackFinished && PreviousState == IdleState)
            .TriggerIf(Trigger.Walk, () => AttackState.AttackFinished && PreviousState == WalkState);

        StateMachine.Configure(ThrowState)
            .SubstateOf(AnyState)
            .Permit(Trigger.Idle, IdleState)
            .Permit(Trigger.Walk, WalkState)
            .Permit(Trigger.Air, AirState)
            .TriggerIf(Trigger.Idle, () => PreviousState == IdleState)
            .TriggerIf(Trigger.Walk, () => PreviousState == WalkState)
            .TriggerIf(Trigger.Air, () => Velocity.y != 0.0f);
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
        StateChanged?.Invoke(previous, next);
    }

    private void InvokeStateChangedEvent(IState previous, IState next)
    {
        switch (next)
        {
            case PlayerIdleState:
                PlayerIdleEvent?.Invoke();
                break;
            case PlayerWalkState:
                PlayerWalkedEvent?.Invoke();
                break;
            case PlayerJumpState:
                PlayerJumpedEvent?.Invoke();
                break;
            case PlayerAirState:
                PlayerInAirEvent?.Invoke();
                break;
            case PlayerAttackState:
                PlayerAttackedEvent?.Invoke();
                break;
            case PlayerThrowState:
                PlayerThrewEvent?.Invoke();
                break;
            case PlayerStunnedState:
                PlayerStunnedEvent?.Invoke();
                break;
            case PlayerDeadState:
                PlayerDiedEvent?.Invoke();
                break;
        }
    }

    public void TakeHit()
    {
        if (!CanTakeHit || !m_CanGetStunned)
            return;

        if (StateMachine.CanFire(Trigger.Stun))
        {
            StateMachine.Fire(Trigger.Stun);
            m_StunnedCooldownTimer.Restart();
        }

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Enemy enemy))
        {
            TakeHit();
        }
    }

    protected internal Vector3 WorldCursorPosition => Camera.main.ScreenToWorldPoint(Input.CursorPosition);


}