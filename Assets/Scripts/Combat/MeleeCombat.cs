using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace TheGame
{
    [RequireComponent(typeof(Entity))]
    public class MeleeCombat : MonoBehaviour
    {
        public bool IsAttacking => m_Attacking;

        public event System.Action OnAttackAnimationComplete;
        public event System.Action<HitResult, GameObject> OnTargetHit; //Target, Result

        [Header("References")]
        [SerializeField] private Animator m_Animator;
        [SerializeField] private MeleeWeapon m_Weapon;

        [Header("Animation Settings")]
        [SerializeField] private bool m_UseAnimationEvents = true;


        [Header("Animation Triggers")]
        [SerializeField] private string m_AttackAnimationTrigger = "StartAttack";

        private Entity m_OwnerEntity;

        private HitData m_CurrentHitData;
        private bool m_Attacking;

        private List<IHittable> m_HitTargetsThisAttack = new List<IHittable>();


        public void StartAttack(HitData hitData)
        {
            if (m_Attacking)
            {
                CancellAttack();
                Debug.LogWarning($"{m_OwnerEntity.gameObject.name} tried to attack while already attacking.");
            }

            m_Attacking = true;
            m_CurrentHitData = hitData;
            m_HitTargetsThisAttack.Clear();

            m_Weapon.RotateInDirection(m_CurrentHitData.Direction); //For collider + sprite if present

            if (m_UseAnimationEvents)
            {
                m_Animator.SetTrigger(m_AttackAnimationTrigger);
            }
            else
            {
                m_Weapon.EnableCollider();
                m_Weapon.InvokeAttack();
            }
        }

        public void CancellAttack(string animationTrigger = null)
        {
            if (!m_Attacking) return;

            m_Attacking = false;

            if (m_UseAnimationEvents && animationTrigger != null)
            {
                m_Animator.SetTrigger(animationTrigger);
            }

            m_Weapon.DisableCollider();
        }

        public void OnAnimationEvent_HitboxActive()
        {
            if (!m_UseAnimationEvents) return;
            m_Weapon.EnableCollider();
        }

        public void OnAnimationEvent_HitboxInactive()
        {
            if (!m_UseAnimationEvents) return;
            m_Weapon.DisableCollider();
        }

        public void OnAnimationEvent_AttackComplete()
        {
            if (!m_UseAnimationEvents) return;

            m_Attacking = false;
            OnAttackAnimationComplete?.Invoke();
        }

        public void HandleWeaponTrigger(IHittable target)
        {
            MonoBehaviour targetMonoBehaviour = target as MonoBehaviour ?? null;

            if (!m_Attacking) return;
            if (!targetMonoBehaviour) return;
            if (targetMonoBehaviour.gameObject == m_OwnerEntity.gameObject) return;
            if (m_HitTargetsThisAttack.Contains(target)) return;

            m_HitTargetsThisAttack.Add(target);

            HitResult result = target.TakeHit(m_CurrentHitData);
            OnTargetHit?.Invoke(result, targetMonoBehaviour.gameObject);
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