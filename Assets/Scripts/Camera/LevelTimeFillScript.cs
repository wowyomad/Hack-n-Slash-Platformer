using TheGame;
using UnityEngine;
using UnityEngine.UI;

public class LevelTimeFillScript : MonoBehaviour
{
    [SerializeField] private Image m_FillImage;

    private float m_Step = 0.05f;
    private float m_CurrentFill = 1.0f;
    private Level m_Level;

    private bool m_Running = false;

    private void OnEnable()
    {
        EventBus<LevelStartedEvent>.OnEvent += Run;
        EventBus<LevelRestartedEvent>.OnEvent += Run;
        EventBus<LevelExitedEvent>.OnEvent += Abort;
    }

    private void OnDisable()
    {
        EventBus<LevelStartedEvent>.OnEvent -= Run;
        EventBus<LevelRestartedEvent>.OnEvent -= Run;
        EventBus<LevelExitedEvent>.OnEvent -= Abort;
    }

    private void Run(Level level)
    {
        m_Running = true;
        m_CurrentFill = 1.0f;
        m_FillImage.fillAmount = 1.0f;
        m_Level = level;
    }

    private void Run(LevelRestartedEvent e) => Run(e.Level);

    private void Run(LevelStartedEvent e) => Run(e.Level);
    private void Abort(LevelExitedEvent e)
    {
        m_Running = false;
        m_CurrentFill = 1.0f;
        m_FillImage.fillAmount = 1.0f;
        m_Level = null;
    }

    void Update()
    {
        if (!m_Running || m_FillImage.fillAmount <= 0.0f) return;

        m_CurrentFill = m_Level.TimeRemaining / m_Level.TimeToComplete;
        m_FillImage.fillAmount = Mathf.Round(m_CurrentFill / m_Step) * m_Step; 
    }
}
