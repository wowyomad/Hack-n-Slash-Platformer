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
public class Enemy : MonoBehaviour, IHittable
{
    [HideInInspector] public CharacterController2D Controller;
    [HideInInspector] public NavAgent2D NavAgent;
    [HideInInspector] private Transform m_SpriteTransform;

    [Header("Stats")]
    [SerializeField] private CharacterMovementStatsSO m_MovementStats;

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

}
