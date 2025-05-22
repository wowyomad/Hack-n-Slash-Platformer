using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using TheGame;
using NUnit.Framework.Interfaces;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MeleeAttack", story: "[Agent] attacks target at [Position]", category: "Action", id: "a9a52b5e84147b09b5e3cfe9ccfec74a")]
public partial class MeleeAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Position;
    [SerializeReference] public BlackboardVariable<HitResult> LastAttackOutcome;
    [SerializeReference] public BlackboardVariable<float> Delay;
    [SerializeReference] public BlackboardVariable<float> AttackDuration = new BlackboardVariable<float>(0.3f);

    public Enemy Self;
    public MeleeCombat Melee;

    private float m_AttackDelayTimer;
    private bool m_AttackInterrupted = false;
    protected override Status OnStart()
    {

        if (LastAttackOutcome == null)
        {
            LogFailure("LastAttackOutcome reference must be set!");
            return Status.Failure;
        }

        Self = Agent.Value.GetComponent<Enemy>();
        Melee = Self.GetComponent<MeleeCombat>();
        m_AttackDelayTimer = 0f;
        m_AttackInterrupted = false;

        if (Melee.IsAttacking)
        {
            Melee.CancellAttack();
        }

        Melee.OnTargetHit += HandleTargetHitResult;

        Attack();

        return Status.Running;
    }

    protected override void OnEnd()
    {
        Melee.OnTargetHit -= HandleTargetHitResult;
    }

    protected override Status OnUpdate()
    {
        if (m_AttackInterrupted)
        {
            return Status.Failure;
        }
        if (!Melee.IsAttacking)
        {
            return Status.Success;
        }

        return Status.Running;
    }

    private void HandleTargetHitResult(HitResult hitResult, GameObject target)
    {
        switch (hitResult)
        {
            case HitResult.Block:
                m_AttackInterrupted = true;
                break;
            case HitResult.Parry:
                m_AttackInterrupted = true;
                break;
            case HitResult.Stun:
                m_AttackInterrupted = true;
                break;
            default:
                break;
        }
        LastAttackOutcome.Value = hitResult;
    }

    private void Attack()
    {
        Vector3 direction = (Position.Value.transform.position - Self.transform.position).normalized;
        HitData hitData = new HitData(GameObject);
        hitData.Direction = direction;

        Self.Flip(direction.x);

        Melee.StartAttack(hitData);
    }
}

