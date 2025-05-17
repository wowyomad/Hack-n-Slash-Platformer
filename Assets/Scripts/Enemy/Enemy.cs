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
    [SerializeField] private float m_EysightAngle = 60.0f;
    [SerializeField] private float m_BacksightDistance = 5.0f;
    [SerializeField] private float m_BacksightAngle = 45.0f;
    [SerializeField] private float m_CloseSightRadius = 3.0f;
    [SerializeField] private float m_AlertedSightRadius = 10.0f;

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

    public bool CanSeeTarget(Vector3 targetPosition, bool alerted = false)
    {
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);


        float closRadius = alerted ? m_AlertedSightRadius : m_CloseSightRadius;

        if (distanceToTarget <= closRadius)
        {
            RaycastHit2D shortHit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, LayerMask.GetMask("Ground", "TransparentGround"));
            return shortHit.collider == null;
        }

        float angleToTarget = Vector3.Angle(transform.right * FacingDirection, directionToTarget);

        bool targetInFront = (targetPosition.x > transform.position.x && FacingDirection == 1) || (targetPosition.x < transform.position.x && FacingDirection == -1);

        float compareDistance;
        float compareAngle;

        if (targetInFront)
        {
            compareDistance = m_EysightDistance;
            compareAngle = m_EysightAngle / 2;
        }
        else
        {
            compareDistance = m_BacksightDistance;
            compareAngle = m_BacksightAngle / 2;
            angleToTarget = 180.0f - angleToTarget;
        }

        if (distanceToTarget > compareDistance || angleToTarget > compareAngle)
        {
            return false;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, LayerMask.GetMask("Ground", "TransparentGround"));
        return hit.collider == null;
    }

    public bool CanSeeEntity(Entity entity, bool alerted = false)
    {
        return CanSeeTarget(entity.transform.position);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        DrawVisionCone(m_EysightDistance, m_EysightAngle, Color.green);

        DrawVisionCone(m_BacksightDistance, m_BacksightAngle, Color.yellow, true);

        DrawVisionCircle(m_CloseSightRadius, Color.blue);
        DrawVisionCircle(m_AlertedSightRadius, Color.red);
    }

    private void DrawVisionCircle(float radius, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void DrawVisionCone(float distance, float angle, Color color, bool isBacksight = false)
    {
        Gizmos.color = color;
        Vector3 forwardDirection = transform.right * FacingDirection * (isBacksight ? -1 : 1);

        Quaternion upRayRotation = Quaternion.AngleAxis(-angle / 2, Vector3.forward);
        Quaternion downRayRotation = Quaternion.AngleAxis(angle / 2, Vector3.forward);

        Vector3 upRayDirection = upRayRotation * forwardDirection;
        Vector3 downRayDirection = downRayRotation * forwardDirection;

        Gizmos.DrawRay(transform.position, upRayDirection * distance);
        Gizmos.DrawRay(transform.position, downRayDirection * distance);


        int segments = 20;
        float segmentAngle = angle / segments;
        Vector3 prevPoint = transform.position + upRayDirection * distance;

        for (int i = 1; i <= segments; i++)
        {
            Quaternion segmentRotation = Quaternion.AngleAxis(segmentAngle * i - angle / 2, Vector3.forward);
            Vector3 currentPointDirection = segmentRotation * forwardDirection;
            Vector3 currentPoint = transform.position + currentPointDirection * distance;
            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }
#endif

}
