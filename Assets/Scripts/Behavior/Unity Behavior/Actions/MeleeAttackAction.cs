using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using TheGame;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MeleeAttack", story: "[Agent] attacks target at [Position]", category: "Action", id: "a9a52b5e84147b09b5e3cfe9ccfec74a")]
public partial class MeleeAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Position;
    [SerializeReference] public BlackboardVariable<HitResult> LastAttackOutcome;
    [SerializeReference] public BlackboardVariable<bool> TookHit;
    [SerializeReference] public BlackboardVariable<List<string>> AttackAnimationTriggers = new BlackboardVariable<List<string>>(new List<string> { "Attack_1" });
    [SerializeReference] public BlackboardVariable<string> StunAnimationTrigger = new BlackboardVariable<string>("Stun");

    protected Enemy Self;
    protected MeleeCombat MeleeController;

    private bool m_AttackInterrupted = false;

    private bool m_Initialized = false;
    protected override Status OnStart()
    {
        if (!m_Initialized && !Initialize())
        {
            return Status.Failure;
        }

        if (LastAttackOutcome == null)
        {
            LogFailure("LastAttackOutcome reference must be set!");
            return Status.Failure;
        }

        m_AttackInterrupted = false;
        LastAttackOutcome.Value = HitResult.None;

        if (MeleeController.IsAttacking)
        {
            MeleeController.CancellAttack();
        }

        MeleeController.OnTargetHit += HandleTargetHitResult;

        Attack();

        return Status.Running;
    }

    protected override void OnEnd()
    {
        MeleeController.OnTargetHit -= HandleTargetHitResult;
    }

    protected override Status OnUpdate()
    {
        if (TookHit.Value)
        {
            LastAttackOutcome.Value = HitResult.Parry;
            m_AttackInterrupted = true;
        }
        if (m_AttackInterrupted || !MeleeController.IsAttacking)
            {
                if (m_AttackInterrupted)
                {
                    if (LastAttackOutcome.Value == HitResult.Parry)
                    {
                        MeleeController.CancellAttack("Stun");
                    }
                }
                return Status.Success;
            }

        return Status.Running;
    }

    private void HandleTargetHitResult(HitResult hitResult, GameObject target)
    {
        switch (hitResult)
        {
            case HitResult.Block:
            case HitResult.Parry:
                m_AttackInterrupted = true;
                break;
            case HitResult.Hit:
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

        //chose random trigger from attack triggers
        string attackTrigger = AttackAnimationTriggers.Value.Count > 0 ? AttackAnimationTriggers.Value[UnityEngine.Random.Range(0, AttackAnimationTriggers.Value.Count)] : null;

        MeleeController.StartAttack(hitData, attackTrigger);
    }

    private bool Initialize()
    {
        if (m_Initialized) return true;

        if (Agent.Value.TryGetComponent<Enemy>(out Self) == false)
        {
            LogFailure("Agent must have an Enemy component!", true);
            return false;
        }
        if (Agent.Value.TryGetComponent<MeleeCombat>(out MeleeController) == false)
        {
            LogFailure("Agent must have a MeleeCombat component!", true);
            return false;
        }

        m_Initialized = true;
        return true;
    }
}

