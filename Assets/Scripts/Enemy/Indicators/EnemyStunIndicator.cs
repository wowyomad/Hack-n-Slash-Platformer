using Unity.Behavior;
using UnityEngine;
using UnityEngine.UI;

namespace TheGame
{
    public class EnemyStunIndicator : MonoBehaviour
    {
        [SerializeField] protected Image FillBar;

        [Header("Blackboard variable names")]
        [SerializeField] private string m_CurrentStateVariableName = "State";
        [SerializeField] private string m_StunDurationVariableName = "StunDuration";
        [SerializeField] private string m_StunTimeLeftVariableName = "StunTimeLeft";

        private BehaviorGraphAgent m_BehaviorGraphAgent;
        private BlackboardVariable<Enemy.State> m_State;
        private BlackboardVariable<float> m_StunDuration;
        private BlackboardVariable<float> m_StunTimeLeft;

        private void Awake()
        {
            m_BehaviorGraphAgent = GetComponentInParent<BehaviorGraphAgent>();
            if (FillBar == null)
            {
                Debug.LogError("FillBar is not assigned in the inspector.", this);
            }
        }

        private void Start()
        {
            m_BehaviorGraphAgent.GetVariable(m_CurrentStateVariableName, out m_State);
            m_BehaviorGraphAgent.GetVariable(m_StunDurationVariableName, out m_StunDuration);
            m_BehaviorGraphAgent.GetVariable(m_StunTimeLeftVariableName, out m_StunTimeLeft);

            m_State.OnValueChanged += OnStateChanged;
            m_StunTimeLeft.OnValueChanged += OnStunTimeLeftChanged;

            gameObject.SetActive(false);
        }

        private void OnStateChanged()
        {
            if (m_State.Value == Enemy.State.Stunned)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnStunTimeLeftChanged()
        {
            if (FillBar != null)
            {
                FillBar.fillAmount = 1.0f - m_StunTimeLeft.Value / m_StunDuration.Value;
            }
        }
    }
}
