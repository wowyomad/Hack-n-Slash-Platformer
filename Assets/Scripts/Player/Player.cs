using System;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(CharacterController2D))]
public class Player : MonoBehaviour, IStateTrackable, IHittable
{

    [Header("Components")]
    protected StateMachine<IPlayerState> m_StateMachine;
    public CharacterController2D Controller;
    public Animator Animator;
    public PlayerAnimationEventBehaviour Animation;
    public InputReader Input;

    public Vector3 Velocity = Vector3.zero;
    public CharacterMovementStats Movement;

    public PlayerIdleState IdleState;
    public PlayerWalkState WalkState;
    public PlayerJumpState JumpState;
    public PlayerAirState AirState;
    public PlayerAttackState AttackState;

    public event Action<IState, IState> StateChanged;
    public event Action Hit;

    public IState CurrentState => m_StateMachine.CurrentState;

    public int FacingDirection { get; private set; }
    public Vector3 WorldCursorPosition => Camera.main.ScreenToWorldPoint(Input.CursorPosition);

    public bool CanTakeHit => m_StateMachine.CurrentState is IPlayerVulnarableState;

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
        m_StateMachine.OnStateChange += InvokeOnStateChange;
        m_StateMachine.OnStateChange += LogStateChange;
    }

    private void OnDisable()
    {
        m_StateMachine.OnStateChange -= InvokeOnStateChange;
        m_StateMachine.OnStateChange -= LogStateChange;

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

    public void TurnToCursor()
    {
        Vector3 mousePosition = Input.CursorPosition;
        Vector3 playerPosition = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 direction = mousePosition - playerPosition;
        Flip(direction.x > 0 ? 1 : -1);
    }

    protected void InitializeStates()
    {
        m_StateMachine = new StateMachine<IPlayerState>();

        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        JumpState = new PlayerJumpState(this);
        AirState = new PlayerAirState(this);
        AttackState = new PlayerAttackState(this);

        m_StateMachine.ChangeState(IdleState);
    }

    public void ChangeState(IPlayerState state)
    {
        m_StateMachine.ChangeState(state);
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

    private void LogStateChange(IState previous, IState next)
    {
        if (previous != null)
            Debug.Log($"Changing state from {previous} to {next}");
        else
            Debug.Log($"Assigning state to {next}");
    }

    private void InvokeOnStateChange(IState previous, IState next)
    {
        StateChanged?.Invoke(previous, next);
    }

    public void TakeHit()
    {
        Hit?.Invoke();
        EventBus<PlayerHitEvent>.Raise(new PlayerHitEvent() { PlayerPosition = transform.position });
    }

    #region likely to be removed
    protected void At(IPlayerState from, IPlayerState to, Func<bool> condition)
    {
        m_StateMachine.AddTransition(from, to, condition);
    }
    protected void At(IPlayerState from, IPlayerState to, IPredicate predicate)
    {
        m_StateMachine.AddTransition(from, to, predicate);
    }
    protected void Any(IPlayerState from, IPlayerState to, IPredicate predicate)
    {
        m_StateMachine.AddAnyTransition(to, predicate);
    }
    #endregion
}