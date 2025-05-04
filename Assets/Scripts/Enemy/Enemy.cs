using System;
using UnityEngine;
using Behavior;
using System.Collections.Generic;
using Unity.VisualScripting;

public interface IDestroyable
{
    public event Action<IDestroyable> OnDestroyed;
}


[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour, IHittable, IDamageable, IDestroyable
{
    [HideInInspector] public CharacterController2D Controller;
    public CharacterMovementStatsSO MovementStats;
    public EnemyBehaviorConfigSO BehaviorConfig;
    public StateMachine<IEnemyState> StateMachine;
    public NavAgent2D Agent;
    public BehaviorTree Tree;

    public Vector3 Velocity;

    [HideInInspector]
    public GameObject PlayerReference;
    [HideInInspector]
    public SpriteRenderer Sprite;

    [SerializeField] private List<Transform> m_WayPoints;
    public bool CanTakeHit => StateMachine.CurrentState is IEnemyVulnarableState;
    public float DistanceToPlayer => Vector3.Distance(transform.position, PlayerReference.transform.position);
    public Vector2 DirectionToPlayer => (PlayerReference.transform.position - transform.position).normalized;
    public bool IsFacingToPlayer => FacingDirection == (DirectionToPlayer.x > 0.0f ? 1 : -1);
    public bool CanSeePlayer => IsPlayerOnSight();
    public int FacingDirection { get; private set; }


    public event Action<float, Vector2> OnTakeDamage;
    public event Action Hit;
    public event Action<IDestroyable> OnDestroyed;

    private void OnEnable()
    {
        Initialize();
    }

    private void Awake()
    {
        Agent = GetComponent<NavAgent2D>();
        Controller = GetComponent<CharacterController2D>();
        PlayerReference = GameObject.FindWithTag("Player");
        Sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        FacingDirection = transform.localScale.x > 0 ? 1 : -1;
    }

    public void Initialize()
    {
        //StateMachine = new StateMachine<IEnemyState>();
        //StateMachine.ChangeState(new StandartEnemyIdleState(this));

        //StateMachine.AddTransition(idle, chase, () => Tree.CurrentBehavior is SeekStrategy;
        //StateMachine.AddTransition(chase, idle, () => Tree.CurrentBehavior is PatrolStrategy);

        Tree = new BehaviorTree();
    }
    private void Update()
    {
        Tree.Execute();
        Flip(Controller.LastDisplacement.x);

        if (Input.GetMouseButtonDown(0))
        {
            Agent.SetDestination(Camera.main.ScreenToWorldPoint(Input.mousePosition));
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
        OnDestroyed?.Invoke(this);
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

        Color rayColor = hit.collider != null ? Color.red : Color.green; // Red if hit, Green if clear
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

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1.0f, 0.3f, 1.0f);
        foreach (var point in m_WayPoints)
        {
            Gizmos.DrawSphere(point.position, 0.25f);
        }

        Gizmos.color = new Color(0.2f, 0.8f, 0.3f, 0.75f);
        Gizmos.DrawLineStrip(m_WayPoints.ConvertAll(point => point.position).ToArray(), false);
    }
}
