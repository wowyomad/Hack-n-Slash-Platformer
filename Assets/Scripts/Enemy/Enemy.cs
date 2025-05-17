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
public class Enemy : Entity, IHittable, IWeaponWielder
{
    [HideInInspector] public CharacterController2D Controller;
    [HideInInspector] public NavAgent2D NavAgent;

    [SerializeField] private float m_EysightDistance = 12.0f;
    [SerializeField] private float m_BacksightDistance = 5.0f;
    [SerializeField] private float m_NearTransparentDistanceThreshold = 2.0f;

    public bool IsStunned => m_Stunned;
    private bool m_Stunned = false;

    public override bool IsAlive => true;


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

        NavAgent.SetTurnCallback(Flip);
    }

    private void Start()
    {
        FacingDirection = transform.localScale.x > 0 ? 1 : -1;
    }

    private void Update()
    {

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

    public bool CanSeeTarget(Vector3 targetPosition)
    {
        int direction = targetPosition.x > transform.position.x ? 1 : -1;
        float compareDistance;
        if (direction != FacingDirection)
        {
            compareDistance = m_EysightDistance;
        }
        else
        {
            compareDistance = m_BacksightDistance;
        }
        float distance = Vector3.Distance(transform.position, targetPosition);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, targetPosition - transform.position, distance, LayerMask.GetMask("Ground", "TransparentGround"));
        return hit.collider == null && distance < compareDistance;
    }

    public bool CanSeeEntity(Entity entity)
    {
        return CanSeeTarget(entity.transform.position);
    }

}
