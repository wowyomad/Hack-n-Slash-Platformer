using TheGame;
using UnityEngine;

public class EntityAnimationEvents : MonoBehaviour
{
    private MeleeCombat m_MeleeCombat;

    private void Awake()
    {
        m_MeleeCombat = GetComponentInParent<MeleeCombat>();   
    }
    public void AttackActive()
    {
        m_MeleeCombat.OnAnimationEvent_HitboxActive();
    }

    public void AttackInactive()
    {
        m_MeleeCombat.OnAnimationEvent_HitboxInactive();
    }

    public void AttackComplete()
    {
        m_MeleeCombat.OnAnimationEvent_AttackComplete();
    }
}
