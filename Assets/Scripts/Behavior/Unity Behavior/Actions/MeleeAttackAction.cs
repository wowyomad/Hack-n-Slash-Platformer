using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using TheGame;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MeleeAttack", story: "[Agent] attacks [Target]", category: "Action", id: "a9a52b5e84147b09b5e3cfe9ccfec74a")]
public partial class MeleeAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Cooldown;
    [SerializeReference] public BlackboardVariable<float> Delay;

    [SerializeReference] public BlackboardVariable<float> AttackDuration = new BlackboardVariable<float>(0.3f);

    public Enemy Self;
    public MeleeWeapon Weapon;

    private float m_AttackTimer;
    private float m_LastAttackTime;
    private float m_AttackDelayTimer;
    protected override Status OnStart()
    {
        {
            Debug.LogError("Under reconstruction");
            return Status.Failure;
        }


        if (Time.time - m_LastAttackTime < Cooldown.Value)
        {
            return Status.Failure;
        }

        Self = Agent.Value.GetComponent<Enemy>();
        Weapon = Self.GetComponentInChildren<MeleeWeapon>();
        m_AttackTimer = 0f;
        m_AttackDelayTimer = 0f;

        Vector3 direction = (Target.Value.transform.position - Self.transform.position).normalized;

        Self.Flip(direction.x);


        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (m_AttackDelayTimer >= Delay.Value)
        {
            if (m_AttackTimer >= AttackDuration.Value)
            {
                return Status.Success;
            }
            m_AttackTimer += Time.deltaTime;
        }
        else
        {
            m_AttackDelayTimer += Time.deltaTime;
        }
        return Status.Running;


    }

    protected override void OnEnd()
    {

    }
}

