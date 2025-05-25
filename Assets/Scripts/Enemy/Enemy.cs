using System;
using UnityEngine;
using TheGame;
using Unity.Behavior;
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
        Idle,
        Locomotion,
        Stunned,
        Knocked,
        Attack,
        Dead,
    }


    public override bool IsAlive => m_CurrentState != null && m_CurrentState.Value != State.Dead;
    public bool IsAlerted => m_AlertedState != null && m_AlertedState.Value == AlertedState.Alerted;

    [HideInInspector] public CharacterController2D Controller;
    [HideInInspector] public MeleeCombat MeleeCombatController;
    [HideInInspector] public NavAgent2D NavAgent;
    [HideInInspector] public BehaviorGraphAgent BTAgent;


    [Header("Blackboard variable names")]
    [SerializeField] private string m_CurrentStateVariableName = "State";
    [SerializeField] private string m_TookHitVariableName = "TookHit";
    [SerializeField] private string m_LastHitResultVariableName = "LastHitResult";
    [SerializeField] private string m_LastHitAttackerVariableName = "LastHitAttacker";
    [SerializeField] private string m_LastHitDirectionVariableName = "LastHitDirection";
    [SerializeField] private string m_AlertedVariableName = "State_Alerted";

    private BlackboardVariable<State> m_CurrentState;
    private BlackboardVariable<AlertedState> m_AlertedState;


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

    }

    private void Start()
    {
        FacingDirection = transform.localScale.x > 0 ? 1 : -1;

        ValidateBlackboardVairiables();
        InitializeVariablesFromBlackboard();

        BTAgent.GetVariable<bool>(m_TookHitVariableName, out var tookHit);
        tookHit.OnValueChanged += OnChanged;
    }
    private void OnChanged()
    {
        BTAgent.GetVariable<bool>(m_TookHitVariableName, out var tookHit);
        Debug.Log($"Took hit: {tookHit.Value}");
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
            case State.Locomotion:
            case State.Idle:
            case State.Stunned:
            case State.Knocked:
                result = HitResult.Hit;
                EventBus<EnemyGotHitEvent>.Raise(new EnemyGotHitEvent { EnemyPosition = transform.position });
                break;
            case State.Attack:
                result = HitResult.Parry;
                EventBus<EnemyGotParriedEvent>.Raise(new EnemyGotParriedEvent { EnemyPosition = attackData.Attacker.transform.position });
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

            BTAgent.SetVariableValue(m_TookHitVariableName, true);
            BTAgent.SetVariableValue(m_LastHitResultVariableName, result);
            BTAgent.SetVariableValue(m_LastHitAttackerVariableName, attackData.Attacker);
            BTAgent.SetVariableValue(m_LastHitDirectionVariableName, attackData.Direction);
        }

        return result;
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
        if (!BTAgent.GetVariable(m_LastHitDirectionVariableName, out BlackboardVariable<Vector2> lastHitDirection))
        {
            Debug.LogError($"Blackboard variable '{m_LastHitDirectionVariableName}' not found");
        }
    }

    private void InitializeVariablesFromBlackboard()
    {
        if (!BTAgent.GetVariable(m_CurrentStateVariableName, out m_CurrentState))
        {
            Debug.LogError($"Blackboard variable '{m_CurrentStateVariableName}' not found");
        }
        if (!BTAgent.GetVariable(m_AlertedVariableName, out m_AlertedState))
        {
            Debug.LogError($"Blackboard variable '{m_AlertedVariableName}' not found");
        }
    }
}