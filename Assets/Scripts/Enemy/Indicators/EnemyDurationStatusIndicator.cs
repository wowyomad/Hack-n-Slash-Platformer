using Unity.Behavior;
using UnityEngine;
using UnityEngine.UI;

namespace TheGame
{
    public class EnemyDurationStatusIndicator : MonoBehaviour
    {
        [SerializeField] protected Image FillBar;
        [SerializeField] private Enemy.State m_ActivationState = Enemy.State.Stunned;

        [Header("Blackboard variable names")]
        [SerializeField] private string m_CurrentStateVariableName = "State";
        [SerializeField] private string m_StateDurationVariableName = "StateDuration";
        [SerializeField] private string m_StateTimeLeftVariableName = "StateTimeLeft";

        private BehaviorGraphAgent m_BehaviorGraphAgent;
        private BlackboardVariable<Enemy.State> m_State;
        private BlackboardVariable<float> m_StateDuration;
        private BlackboardVariable<float> m_StateTimeLeft;

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
            m_BehaviorGraphAgent.GetVariable(m_StateDurationVariableName, out m_StateDuration);
            m_BehaviorGraphAgent.GetVariable(m_StateTimeLeftVariableName, out m_StateTimeLeft);

            m_State.OnValueChanged += OnStateChanged;
            m_StateTimeLeft.OnValueChanged += OnStateTimeLeftChanged;

            gameObject.SetActive(false);
        }

        private void OnStateChanged()
        {
            if (m_State.Value == m_ActivationState)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnStateTimeLeftChanged()
        {
            if (FillBar != null && m_StateDuration.Value > 0)
            {
                FillBar.fillAmount = 1.0f - m_StateTimeLeft.Value / m_StateDuration.Value;
            }
        }
    }
}
