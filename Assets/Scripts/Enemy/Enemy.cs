using System;
using UnityEngine;
using Behavior;
using System.Collections.Generic;
using static UnityEngine.UI.Image;
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
    public CharacterMovementStats Movement;
    public EnemyBehaviorConfigSO BehaviorConfig;
    public StateMachine<IEnemyState> StateMachine;
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
        Controller = GetComponent<CharacterController2D>();
        PlayerReference = GameObject.FindWithTag("Player");
        Sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        FacingDirection = transform.localScale.x > 0 ? 1 : -1;
    }

    private void Update()
    {
        //StateMachine.Update();
        //Controller.Move(Velocity * Time.deltaTime);


        Tree.Execute();
        ApplyGravityToVelocity();
        Controller.Move(Velocity * Time.deltaTime);

        Flip(Controller.LastDisplacement.x);
        string facing = FacingDirection > 0 ? "Right" : "Left";
        string direction = DirectionToPlayer.x > 0 ? "Right" : "Left";
        Debug.Log($"Facing: {facing}, ToPlayer: {IsFacingToPlayer} (Direction To Player: {direction}");
    }

    public void Initialize()
    {
        //StateMachine = new StateMachine<IEnemyState>();
        //StateMachine.ChangeState(new StandartEnemyIdleState(this));

        Tree = new BehaviorTreeBuilder("Enemy")
            .PrioritySelector()
                .Sequence("Chase player", 1)
                    .Condition("Is player nearby?", () =>
                         CanSeePlayer && ((DistanceToPlayer < BehaviorConfig.VisualSeekDistance && IsFacingToPlayer)
                         || DistanceToPlayer < BehaviorConfig.CloseSeekDistance))
                    .Do("Change color to red", () => Sprite.color = new Color(1.0f, 0.2f, 0.3f))
                    .UntilFail().Do("Seek player", new SeekStrategy(this, PlayerReference.transform))
                    .Do("Change color back to normal", () => Sprite.color = new Color(1.0f, 1.0f, 1.0f))
                .End()
                .Do("Patrol", 0, new PatrolStrategy(this, m_WayPoints))
            .End()
        .Build();
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

    public void ApplyGravityToVelocity()
    {
        Velocity.y += Movement.Gravity * Time.deltaTime;
        Velocity.y = Mathf.Max(Velocity.y, Velocity.y, Movement.MaxGravityVelocity);
        if (Controller.IsGrounded)
        {
            Velocity.y = 0.0f;
        }
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }



    private void MoveToPlayer()
    {
        float distance = DistanceToPlayer;
        if (distance > 0.01f)
        {
            Vector3 direction = (PlayerReference.transform.position - transform.position).normalized;
            Vector3 displacement = direction * Movement.HorizontalSpeed * Time.deltaTime;
            if (displacement.magnitude > distance)
            {
                displacement = direction * distance;
            }

            Controller.Move(displacement);
        }
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
