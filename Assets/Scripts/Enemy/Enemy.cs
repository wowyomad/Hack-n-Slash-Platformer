using System;
using UnityEngine;
using Behavior;
using TheGame;
using Unity.VisualScripting;

public interface IDestroyable
{
    public event Action<IDestroyable> OnDestroy;
}


[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(NavAgent2D))]
public class Enemy : MonoBehaviour, IHittable, IWeaponWielder
{
    [HideInInspector] public CharacterController2D Controller;
    [HideInInspector] public NavAgent2D NavAgent;
    [HideInInspector] private Transform m_SpriteTransform;

    [SerializeField] private float m_EysightDistance = 8.0f;

    public bool IsStunned => m_Stunned;
    private bool m_Stunned = false;


    [Header("Other")]


    #region convinience
    public int FacingDirection { get; private set; }
    #endregion

    #region flags
    public bool CanTakeHit { get; private set; } = true;
    #endregion

    #region events
    public event Action OnHit;
    #endregion

    private void Awake()
    {
        NavAgent = GetComponent<NavAgent2D>();
        Controller = GetComponent<CharacterController2D>();

        Initialize();
    }

    private void Start()
    {
        FacingDirection = transform.localScale.x > 0 ? 1 : -1;
    }

    public void Initialize()
    {
        SpriteRenderer spriteRenderer;
        if (TryGetComponent(out spriteRenderer))
        {
            m_SpriteTransform = spriteRenderer.transform;
        }
        else if ((spriteRenderer = GetComponentInChildren<SpriteRenderer>()) != null)
        {
            m_SpriteTransform = spriteRenderer.transform;
        }
        else
        {
            throw new MissingComponentException("No SpriteRenderer found on this object or its children.");
        }
    }

    private void Update()
    {
        Flip(NavAgent.Velocity.x);
    }

    public HitResult TakeHit()
    {
        if (CanTakeHit)
        {
            OnHit?.Invoke();
            EventBus<EnemyHitEvent>.Raise(new EnemyHitEvent { EnemyPosition = transform.position });

            return HitResult.Hit;
        }
        return HitResult.Nothing;
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

    public void Flip(float direction)
    {
        if (direction == 0.0f)
        {
            return;
        }
        Flip(direction > 0.0f ? 1 : -1);
    }

    public void TryHitTarget(IHittable target)
    {
        if (target == null) return;

        HitResult status = target.TakeHit();
        if (target is Player player)
        {
            Debug.Log($"Hit {player.name} with status {status}");
        }
    }

    public bool CanSeePlayer(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, targetPosition - transform.position, distance, LayerMask.GetMask("Ground") | LayerMask.GetMask("TransparentGround"));
        return hit.collider == null && distance < m_EysightDistance;
    }

    public bool CanSeePlayer(Player player)
    {
        if (player.CurrentState is PlayerDeadState)
        {
            return false;
        }
        Vector3 targetPosition = player.transform.position;
        float distance = Vector3.Distance(transform.position, targetPosition);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, targetPosition - transform.position, distance, LayerMask.GetMask("Ground") | LayerMask.GetMask("TransparentGround"));
        return hit.collider == null && distance < m_EysightDistance;
    }

}
