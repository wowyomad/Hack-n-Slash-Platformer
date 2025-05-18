using System.Collections.Generic;
using UnityEngine;

namespace TheGame
{
    [RequireComponent(typeof(Entity))]
    public class MeleeCombat : MonoBehaviour
    {
        public bool IsAttacking => m_Attacking;

        public event System.Action OnAttackAnimationComplete;
        public event System.Action<HitResult, GameObject> OnTargetHitProcessed; //Target, Result

        [Header("References")]
        [SerializeField] private Animator m_Animator;
        [SerializeField] private MeleeWeapon m_Weapon;


        [Header("Animation Triggers")]
        [SerializeField] private string m_AttackAnimationTrigger = "StartAttack";
        [SerializeField] private string m_CancelAttackAnimationTrigger = "CancelAttack";

        private Entity m_OwnerEntity;

        private HitData m_CurrentHitData;
        private bool m_Attacking;

        private List<IHittable> m_HitTargetsThisAttack = new List<IHittable>();


        public void StartAttack(HitData hitData)
        {
            m_Attacking = true;
            m_CurrentHitData = hitData;

            m_Animator.SetTrigger(m_AttackAnimationTrigger);
        }

        public void CancellAttack()
        {
            if (!m_Attacking) return;

            m_Attacking = false;
            m_Animator.SetTrigger("CancelAttack");
        }

        public void OnAnimationEvent_HitboxActive()
        {
            m_Weapon.EnableCollider();
        }

        public void OnAnimationEvent_HitboxInactive()
        {
            m_Weapon.DisableCollider();
        }

        public void OnAnimationEvent_AttackComplete()
        {
            m_Attacking = false;
            OnAttackAnimationComplete?.Invoke();
        }

        public void HandleWeaponTrigger(IHittable target)
        {
            MonoBehaviour targetMonoBehaviour = target as MonoBehaviour ?? null;

            if (!targetMonoBehaviour) return;
            if (!m_Attacking) return;
            if (targetMonoBehaviour.gameObject == m_OwnerEntity.gameObject) return;
            if (m_HitTargetsThisAttack.Contains(target)) return;

            m_HitTargetsThisAttack.Add(target);

            HitResult result = target.TakeHit(m_CurrentHitData);

            ProcessHitResult(targetMonoBehaviour.gameObject, result);

            OnTargetHitProcessed?.Invoke(result, targetMonoBehaviour.gameObject);
        }

        public void ProcessHitResult(GameObject target, HitResult result)
        {
            switch (result)
            {
                case HitResult.Hit:
                    Debug.Log($"{gameObject.name} hit {target.name} successfully.");
                    break;
                case HitResult.Block:
                    Debug.Log($"{gameObject.name}'s attack was Blocked by {target.name}.");
                    break;
                case HitResult.Parry:
                    Debug.Log($"{gameObject.name}'s attack was Parried by {target.name}.");
                    break;
                case HitResult.Nothing:
                    Debug.Log($"{gameObject.name}'s attack did nothing to {target.name}.");
                    break;
            }
        }


        private void Awake()
        {
            if (!m_Animator) m_Animator = GetComponentInChildren<Animator>();
            if (!m_Weapon) m_Weapon = GetComponentInChildren<MeleeWeapon>();
            m_OwnerEntity = GetComponent<Entity>();

            if (!m_Animator) Debug.LogError($"{gameObject.name}: MeleeCombat requires an Animator child.", this);
            if (!m_Weapon) Debug.LogError($"{gameObject.name}: MeleeCombat requries a Weapon child with a Weapon script.", this);

            if (m_Weapon) m_Weapon.SetCombatController(this);
        }
    }
}