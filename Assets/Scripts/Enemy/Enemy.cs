using System;
using UnityEngine;
using Behavior;

public interface IDestroyable
{
    public event Action<IDestroyable> DestroyedEvent;
}


[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour, IHittable, IDamageable, IDestroyable
{
    [HideInInspector] public CharacterController2D Controller;
    public CharacterMovementStatsSO MovementStats;
    public EnemyBehaviorConfigSO BehaviorConfig;
    public TheGame.OldStateMachine<IEnemyState> StateMachine;
    public NavAgent2D Agent;
    public BehaviorTree Tree;

    public Vector3 Velocity;

    public GameObject PlayerReference;
    public Player Player;
    [HideInInspector]
    public SpriteRenderer Sprite;

    public bool CanTakeHit => StateMachine.CurrentState is IEnemyVulnarableState;
    public float DistanceToPlayer => Vector3.Distance(transform.position, PlayerReference.transform.position);
    public Vector2 DirectionToPlayer => (PlayerReference.transform.position - transform.position).normalized;
    public bool IsFacingToPlayer => FacingDirection == (DirectionToPlayer.x > 0.0f ? 1 : -1);
    public bool CanSeePlayer => IsPlayerOnSight();
    public bool AlwaysFollowPlayer = false;
    public int FacingDirection { get; private set; }

    private Vector3 m_LastPlayerPosition;
    private bool m_SeenPlayer = false;


    public event Action<float, Vector2> OnTakeDamage;
    public event Action Hit;
    public event Action<IDestroyable> DestroyedEvent;

    private void OnEnable()
    {
        Initialize();
    }

    private void Awake()
    {
        Agent = GetComponent<NavAgent2D>();
        Controller = GetComponent<CharacterController2D>();
        PlayerReference = GameObject.FindWithTag("Player");
        Player = PlayerReference.GetComponent<Player>();
        Sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        FacingDirection = transform.localScale.x > 0 ? 1 : -1;

        m_FollowPlayerTimer.SetFinishedCallback(SetDestinationToPlayer);
        m_FollowPlayerTimer.Start(0.25f);
    }

    public void Initialize()
    {
        Tree = new BehaviorTree();
    }

    private ActionTimer m_FollowPlayerTimer = new ActionTimer(true, false);
    private void Update()
    {
        Tree.Execute();
        m_FollowPlayerTimer.Tick();

        if (CanSeePlayer || DistanceToPlayer <= 7.5f)
        {
            m_SeenPlayer = true;
            m_LastPlayerPosition = PlayerReference.transform.position;
        }

        Flip(Controller.LastDisplacement.x);
    }

    void SetDestinationToPlayer()
    {
        if (!Controller.IsGrounded) return;

        if (CanSeePlayer || DistanceToPlayer <= 5.0f)
        {
            Agent.SetDestination(PlayerReference.transform.position);
        }
        else if (m_SeenPlayer && Vector3.Distance(m_LastPlayerPosition, transform.position) > 1.0f)
        {
            Agent.SetDestination(m_LastPlayerPosition);
        }
    }

    public void TakeDamage(float value, Vector2 normal)
    {
        OnTakeDamage?.Invoke(value, normal);
    }
    public void TakeHit()
    {
        if (CanTakeHit)
        {
            Hit?.Invoke();
            EventBus<EnemyHitEvent>.Raise(new EnemyHitEvent { EnemyPosition = transform.position });
        }
    }

    private void OnDestroy()
    {
        DestroyedEvent?.Invoke(this);
        StateMachine.OnDestroy();
    }

    public void Flip(int direction)
    {
        if (direction == 0)
        {
            return;
        }
        direction = direction > 0 ? 1 : -1;

        if (direction != FacingDirection)
        {
            FacingDirection = -FacingDirection;
            transform.localScale = new Vector3(FacingDirection, 1.0f, 1.0f);
        }
    }

    private bool IsPlayerOnSight()
    {
        int layerMask = LayerMask.GetMask("Ground") | LayerMask.GetMask("TransparentGround");

        Vector3 direction = DirectionToPlayer;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, DistanceToPlayer, layerMask);

        Color rayColor = hit.collider != null ? Color.red : Color.green;
        Debug.DrawRay(transform.position, direction * DistanceToPlayer, rayColor);


        return hit.collider == null;
    }

    public void Flip(float direction)
    {
        if (direction == 0.0f)
        {
            return;
        }
        Flip(direction > 0.0f ? 1 : -1);
    }

}
