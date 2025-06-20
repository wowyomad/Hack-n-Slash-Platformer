using System;
using UnityEngine;
using GameActions;
using System.Collections.Generic;
using System.Linq;

namespace TheGame
{
    [RequireComponent(typeof(RageMeter))]
    [RequireComponent(typeof(CharacterController2D))]
    public class Player : Entity, IStateTrackable
    {
        [Header("Children Components")]
        [HideInInspector] public MeleeWeapon WeaponReference;
        [HideInInspector] public MeleeCombat MeleeCombat;
        [HideInInspector] public RageMeter RageMeter;


        private ThrowablePicker m_ThrowablePicker;

        public IThrowable ThrowableInHand;

        public event Action<bool> WhenDash;
        public event Action<bool> WhenAttack;

        public void Internal_OnDashPerofmred(bool performed) => WhenDash?.Invoke(performed);
        public void Internal_OnAttackPerofmred(bool performed) => WhenAttack?.Invoke(performed);

        public override bool IsAlive => IsInvincible || CurrentState != DeadState;

        public bool IsInvincible = false;

        public IState CurrentState => StateMachine.State;
        public enum Trigger
        {
            Idle,
            Walk,
            Jump,
            Air,
            Dash,
            Attack,
            Throw,
            Die,
            Stun,
            Dead,
        }

        #region Events
        public event Action OnPlayerWalk;
        public event Action OnPlayerIdle;
        public event Action OnPlayerJump;
        public event Action OnPlayerAir;
        public event Action OnPlayerDash;
        public event Action OnPlayerAttack;
        public event Action OnPlayerThrow;
        public event Action OnPlayerDead;
        public event Action OnPlayerStunned;
        public event Action OnPlayerClimbedDown;
        #endregion


        #region timers
        private ActionTimer m_StunnedCooldownTimer;
        private ActionTimer m_AttackCoodownTimer;
        private ActionTimer m_DashCooldonwTimer;

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
        protected PlayerDashState DashState;
        protected PlayerThrowState ThrowState;
        #endregion

        [SerializeField] private LayerMask m_EnemyLayerMask;

        [HideInInspector] public StateMachine<PlayerBaseState, Trigger> StateMachine;
        private bool m_StatesInitialized = false;

        [HideInInspector] public CharacterController2D Controller;
        [HideInInspector] public InputReader Input;
        protected PlayerBaseState PreviousState => StateMachine.PreviousState;

        public Vector3 Velocity => Controller.Velocity;
        public CharacterStatsSO Stats;
        public bool IsVulnerable = true;

        public event Action<IState, IState> StateChanged;
        public override event Action OnHit;

        public bool IsGrounded => Controller.IsGrounded;
        public bool CanPickupKnifes = false;
        public bool CanTakeHit => !IsVulnerable;
        private float m_StunCooldownDuration = 1.5f;

        private bool m_CanGetStunned = false;

        private List<ActionTimer> m_Timers;

        private void Awake()
        {
            Input = InputReader.Load();
            Controller = GetComponent<CharacterController2D>();
            WeaponReference = GetComponentInChildren<MeleeWeapon>();
            MeleeCombat = GetComponent<MeleeCombat>();
            RageMeter = GetComponent<RageMeter>();
            m_ThrowablePicker = GetComponent<ThrowablePicker>();

            InitializeFiels();
            SetupTimers();
            SetupStates();
        }


        private void SetupTimers()
        {
            m_StunnedCooldownTimer.SetDuration(m_StunCooldownDuration);
            m_StunnedCooldownTimer.SetFinishedCallback(() => m_CanGetStunned = true);
            m_StunnedCooldownTimer.SetStartedCallback(() => m_CanGetStunned = false);

            m_AttackCoodownTimer.SetDuration(Stats.AttackCooldown);
            m_DashCooldonwTimer.SetDuration(Stats.DashCooldown);
        }

        private void Start()
        {
            FacingDirection = transform.localScale.x > 0 ? 1 : -1;

            Controller.ApplyGravity = true;
            Controller.Gravity = Stats.Gravity;
            Controller.MaxGravityVelocity = Stats.MaxGravityVelocity;
        }

        private void OnEnable()
        {
            Input.ListenEvents(this);

            StateMachine.StateChangedEvent += OnStateChanged;
            StateMachine.StateChangedEvent += InvokeStateChangedEvent;
        }

        private void OnDisable()
        {
            if (!m_StatesInitialized)
                return;

            Input.StopListening(this);

            StateMachine.StateChangedEvent -= OnStateChanged;
        }



        public void Update()
        {
            StateMachine.Update();
            m_Timers.ForEach(timer => timer.Tick());
        }

        public void TurnToCursor()
        {
            Vector3 mousePosition = Input.CursorPosition;
            Vector3 playerPosition = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 direction = mousePosition - playerPosition;
            Flip(direction.x > 0 ? 1 : -1);
        }

        protected void SetupStates()
        {
            if (m_StatesInitialized)
                return;

            m_StatesInitialized = true;

            AnyState = new PlayerAnyState(this);
            StunnedState = new PlayerStunnedState(this, 0.5f);
            DeadState = new PlayerDeadState(this);
            IdleState = new PlayerIdleState(this);
            WalkState = new PlayerWalkState(this);
            JumpState = new PlayerJumpState(this);
            AirState = new PlayerAirState(this);
            DashState = new PlayerDashState(this);
            AttackState = new PlayerAttackState(this);
            ThrowState = new PlayerThrowState(this);

            StateMachine = new StateMachine<PlayerBaseState, Trigger>(IdleState);

            StateMachine.Configure(AnyState)
                .IgnoreIf(Trigger.Die, () => !IsVulnerable)
                .PermitIf(Trigger.Die, DeadState, () => !IsInvincible);
            //.PermitIf(Trigger.Stun, StunnedState, () => m_CanGetStunned && !StunnedState.IsStunned && !IsImmuneToStun);

            StateMachine.Configure(StunnedState)
                .SubstateOf(AnyState)
                .Permit(Trigger.Idle, IdleState)
                .TriggerIf(Trigger.Idle, () => !StunnedState.IsStunned);

            StateMachine.Configure(IdleState)
                .SubstateOf(AnyState)
                .PermitIf(Trigger.Dash, DashState, () => m_DashCooldonwTimer.IsFinished)
                .PermitIf(Trigger.Attack, AttackState, () => m_AttackCoodownTimer.IsFinished)
                .Permit(Trigger.Jump, JumpState)
                .PermitIf(Trigger.Throw, ThrowState, () => ThrowableInHand != null)
                .Permit(Trigger.Air, AirState)
                .Permit(Trigger.Walk, WalkState)
                .TriggerIf(Trigger.Air, () => !Controller.IsGrounded)
                .TriggerIf(Trigger.Walk, () => Input.Horizontal != 0.0f && !WouldMoveIntoWall(Input.Horizontal));

            StateMachine.Configure(WalkState)
                .SubstateOf(AnyState)
                .Permit(Trigger.Jump, JumpState)
                .PermitIf(Trigger.Attack, AttackState, () => m_AttackCoodownTimer.IsFinished)
                .PermitIf(Trigger.Dash, DashState, () => m_DashCooldonwTimer.IsFinished)
                .PermitIf(Trigger.Throw, ThrowState, () => ThrowableInHand != null)
                .Permit(Trigger.Air, AirState)
                .TriggerIf(Trigger.Air, () => !Controller.IsGrounded)
                .Permit(Trigger.Idle, IdleState)
                .TriggerIf(Trigger.Idle, () => Input.Horizontal == 0.0f && Controller.IsGrounded || WouldMoveIntoWall(Input.Horizontal));

            StateMachine.Configure(JumpState)
                .SubstateOf(AnyState)
                .PermitIf(Trigger.Throw, ThrowState, () => ThrowableInHand != null)
                .Permit(Trigger.Air, AirState)
                .PermitIf(Trigger.Dash, DashState, () => m_DashCooldonwTimer.IsFinished)
                .PermitIf(Trigger.Attack, AttackState, () => m_AttackCoodownTimer.IsFinished)
                .TriggerIf(Trigger.Air, () => Controller.Velocity.y <= 0.0f);

            StateMachine.Configure(AirState)
                .SubstateOf(AnyState)
                .Permit(Trigger.Throw, ThrowState)
                .PermitIf(Trigger.Dash, DashState, () => m_DashCooldonwTimer.IsFinished)
                .Permit(Trigger.Idle, IdleState)
                .PermitIf(Trigger.Attack, AttackState, () => m_AttackCoodownTimer.IsFinished)
                .TriggerIf(Trigger.Idle, () => Controller.IsGrounded && Velocity.y == 0.0f);

            StateMachine.Configure(AttackState)
                .SubstateOf(AnyState)
                .PermitIf(Trigger.Idle, IdleState, () => AttackState.AttackFinished)
                .PermitIf(Trigger.Walk, WalkState, () => AttackState.AttackFinished)
                .PermitIf(Trigger.Air, AirState, () => AttackState.AttackFinished)
                .PermitIf(Trigger.Dash, DashState, () => m_DashCooldonwTimer.IsFinished)
                .TriggerIf(Trigger.Idle, () => PreviousState == IdleState)
                .TriggerIf(Trigger.Walk, () => PreviousState == WalkState)
                .TriggerIf(Trigger.Air, () => PreviousState == AirState || PreviousState == JumpState);

            StateMachine.Configure(DashState)
                .SubstateOf(AnyState)
                .PermitIf(Trigger.Air, AirState, () => DashState.DashFinished)
                .PermitIf(Trigger.Idle, IdleState, () => DashState.DashFinished)
                .TriggerIf(Trigger.Idle, () => Controller.IsGrounded)
                .TriggerIf(Trigger.Air, () => !Controller.IsGrounded);

            StateMachine.Configure(ThrowState)
                .SubstateOf(AnyState)
                .Permit(Trigger.Idle, IdleState)
                .Permit(Trigger.Walk, WalkState)
                .Permit(Trigger.Air, AirState)
                .TriggerIf(Trigger.Idle, () => PreviousState == IdleState)
                .TriggerIf(Trigger.Walk, () => PreviousState == WalkState)
                .TriggerIf(Trigger.Air, () => Velocity.y != 0.0f);

            StateMachine.Configure(DeadState);
        }

        public void Throw()
        {
            if (ThrowableInHand != null)
            {
                Throw(ThrowableInHand);
                ThrowableInHand = null;
            }
            else
            {
                Debug.LogWarning("No throwable item in hand to throw.");
            }
        }

        public bool TryPickUpItem()
        {
            if (m_ThrowablePicker.PickablesInArea.Count > 0)
            {
                var item = m_ThrowablePicker.PickablesInArea[0];
                if (!CanPickupKnifes)
                {
                    Debug.LogWarning("Cannot pick up knives while CanPickupKnifes is false.");
                    return false;
                }

                IThrowable pickedItem = item.Pickup(m_ThrowablePicker) as IThrowable;
                if (pickedItem != null)
                {
                    ThrowableInHand = pickedItem;
                    return true;
                }
            }
            else
            {
                Debug.LogWarning("No throwable item found to throw.");
            }
            return false;

        }

        private void Throw(IThrowable throwable)
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
            string stateName = next.GetType().Name.Replace("State", string.Empty);
            string eventName = $"On{stateName}";

            var eventField = GetType().GetField(eventName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (eventField != null && eventField.GetValue(this) is Action eventAction)
            {
                eventAction.Invoke();
            }
        }

        private void Dash()
        {
            if (Can(Trigger.Dash))
            {
                StateMachine.Fire(Trigger.Dash);
                m_DashCooldonwTimer.Restart();
            }
        }

        private void Attack()
        {
            if (Can(Trigger.Attack))
            {
                Do(Trigger.Attack);
                m_AttackCoodownTimer.Restart();
            }
        }

        private void ClimbDown()
        {
            Controller.ClimbDown();
        }

        public override HitResult TakeHit(HitData hitData)
        {
            if (IsDead) return HitResult.Nothing;

            HitResult hitResult = HitResult.Nothing;

            if (CurrentState is PlayerAttackState attack && attack.LastHitTarget == hitData.Attacker && hitData.IsParryable)
            {
                hitResult = HitResult.Parry;
            }
            else if (Can(Trigger.Die))
            {
                hitResult = HitResult.Hit;
                Do(Trigger.Die);

                OnHit?.Invoke();

                EventBus<PlayerGotHitEvent>.Raise(new PlayerGotHitEvent() { PlayerPosition = transform.position });
                EventBus<PlayerDiedEvent>.Raise(new PlayerDiedEvent());
            }

            return hitResult;
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
            Attack();
        }

        [GameAction(ActionType.Throw)]
        protected void HandleThrowInput()
        {
            if (StateMachine.CanFire(Trigger.Throw))
                StateMachine.Fire(Trigger.Throw);
            else if (m_ThrowablePicker.PickablesInArea.Count > 0)
            {
                TryPickUpItem();
            }
        }

        [GameAction(ActionType.Dash)]
        protected void HandleDashInput()
        {
            Dash();
        }


        [GameAction(ActionType.ClimbDown)]
        protected void HandleClimbDownInput()
        {
            ClimbDown();
        }


        private bool Can(Trigger trigger) => StateMachine.CanFire(trigger);
        private void Do(Trigger trigger) => StateMachine.Fire(trigger);

        private void InitializeFiels()
        {
            var timers = GetType()
                     .GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     .Where(field => field.FieldType == typeof(ActionTimer))
                     .ToList();

            foreach (var field in timers)
            {
                var timer = new ActionTimer();
                field.SetValue(this, timer);
            }

            m_Timers = timers.Select(field => (ActionTimer)field.GetValue(this)).ToList();
        }

        private bool WouldMoveIntoWall(float direction)
        {
            if (direction == 0.0f)
                return false;

            if (Controller.IsFacingWallLeft && direction < 0.0f)
                return true;
            else if (Controller.IsFacingWallRight && direction > 0.0f)
                return true;

            return false;
        }
        protected internal Vector3 WorldCursorPosition => Camera.main.ScreenToWorldPoint(Input.CursorPosition);
    }
}
