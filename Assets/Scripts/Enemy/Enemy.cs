using System;
using UnityEngine;
using TheGame;
using Unity.Behavior;
using Unity.VisualScripting;

public interface IDestroyable
{
    public event Action<IDestroyable> OnDestroy;
}

[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(MeleeCombat))]
[RequireComponent(typeof(NavAgent2D))]
[RequireComponent(typeof(BehaviorGraphAgent))]
public class Enemy : Entity
{
    [BlackboardEnum]
    public enum State
    {
        NotAttack,
        Attack,
        Dead,
    }

    public override bool IsAlive => m_CurrentState != null && m_CurrentState.Value != State.Dead;

    [HideInInspector] public CharacterController2D Controller;
    [HideInInspector] public MeleeCombat MeleeCombatController;
    [HideInInspector] public NavAgent2D NavAgent;
    [HideInInspector] public BehaviorGraphAgent BTAgent;

    [SerializeField] private float m_EysightDistance = 12.0f;
    [SerializeField] private float m_EysightAngle = 60.0f;
    [SerializeField] private float m_BacksightDistance = 5.0f;
    [SerializeField] private float m_BacksightAngle = 45.0f;
    [SerializeField] private float m_CloseSightRadius = 3.0f;
    [SerializeField] private float m_AlertedSightRadius = 10.0f;


    [Header("Blackboard variable names")]
    [SerializeField] private string m_CurrentStateVariableName = "CurrentState";
    [SerializeField] private string m_TookHitVariableName = "TookHit";
    [SerializeField] private string m_LastHitResultVariableName = "LastHitResult";
    [SerializeField] private string m_LastHitAttackerVariableName = "LastHitAttacker";

    private BlackboardVariable<State> m_CurrentState;


    #region events
    public override event System.Action OnHit;
    #endregion

    private void Awake()
    {
        NavAgent = GetComponent<NavAgent2D>();
        NavAgent.SetTurnCallback(Flip);

        Controller = GetComponent<CharacterController2D>();
        MeleeCombatController = GetComponent<MeleeCombat>();
        BTAgent = GetComponent<BehaviorGraphAgent>();

        ValidateBlackboardVairiables();
        InitializeVariablesFromBlackboard();
    }

    private void Start()
    {
        FacingDirection = transform.localScale.x > 0 ? 1 : -1;
    }

    private void Update()
    {
        
    }

    public override HitResult TakeHit(HitData attackData)
    {
        if (IsDead) return HitResult.Nothing;

        HitResult result = HitResult.Hit;


        switch (m_CurrentState.Value)
        {
            case State.NotAttack:
                result = HitResult.Hit;
                break;
            case State.Attack:
                result = HitResult.Stun;
                break;
            case State.Dead:
                result = HitResult.Nothing;
                break;
            default:
                Debug.LogError($"State {m_CurrentState.Value} not implemented");
                break;
        }

        if (result != HitResult.Nothing)
        {
            OnHit?.Invoke();
            EventBus<EnemyHitEvent>.Raise(new EnemyHitEvent { EnemyPosition = transform.position });

            BTAgent.SetVariableValue(m_TookHitVariableName, true);
            BTAgent.SetVariableValue(m_LastHitResultVariableName, result);
            BTAgent.SetVariableValue(m_LastHitAttackerVariableName, attackData.Attacker);
        }

        return result;
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

    private void ValidateBlackboardVairiables()
    {
        if (!BTAgent.GetVariable(m_TookHitVariableName, out BlackboardVariable<bool> tookHit))
        {
            Debug.LogError($"Blackboard variable '{m_TookHitVariableName}' not found");
        }

        if (!BTAgent.GetVariable(m_LastHitResultVariableName, out BlackboardVariable<HitResult> lastHitResult))
        {
            Debug.LogError($"Blackboard variable '{m_LastHitResultVariableName}' not found");
        }

        if (!BTAgent.GetVariable(m_LastHitAttackerVariableName, out BlackboardVariable<GameObject> lastHitAttacker))
        {
            Debug.LogError($"Blackboard variable '{m_LastHitAttackerVariableName}' not found");
        }
    }

    private void InitializeVariablesFromBlackboard()
    {
        if (!BTAgent.GetVariable(m_CurrentStateVariableName, out m_CurrentState))
        {
            Debug.LogError($"Blackboard variable '{m_CurrentStateVariableName}' not found");
        }
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