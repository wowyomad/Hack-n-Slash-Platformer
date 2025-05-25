using Unity.Behavior;
using UnityEngine;

namespace TheGame
{
public class EnemyAlertIndicator : MonoBehaviour
{
    [Header("Blackboard Variable Name")]
    [SerializeField] private string m_AlertedStateVariableName = "State_Alerted";

    private BehaviorGraphAgent m_BehaviorGraphAgent;
    private BlackboardVariable<AlertedState> m_AlertedState;

    private void Awake()
    {
        m_BehaviorGraphAgent = GetComponentInParent<BehaviorGraphAgent>();
        if (m_BehaviorGraphAgent == null)
        {
            Debug.LogError("EnemyAlertIndicator requires a BehaviorGraphAgent in its parent objects.", this);
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        bool variableFound = m_BehaviorGraphAgent.GetVariable(m_AlertedStateVariableName, out m_AlertedState);

        if (!variableFound || m_AlertedState == null)
        {
            Debug.LogError($"Blackboard variable '{m_AlertedStateVariableName}' of type AlertedState not found on BehaviorGraphAgent.", this);
            enabled = false;
            return;
        }

        m_AlertedState.OnValueChanged += OnAlertedStateChanged;
        OnAlertedStateChanged();
    }

    private void OnDestroy()
        {
            if (m_AlertedState != null)
            {
                m_AlertedState.OnValueChanged -= OnAlertedStateChanged;
            }
        }

    private void OnAlertedStateChanged()
    {
        if (m_AlertedState == null) return;

        if (m_AlertedState.Value == AlertedState.Alerted)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }
        else
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }
    }
}

}
