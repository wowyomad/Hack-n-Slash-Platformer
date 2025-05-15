using UnityEngine;


public class GameManager : PersistentSingleton<GameManager>
{

    public bool IsGamePaused => m_Paused;

    private InputReader m_Input;

    private float m_OriginalTimeScale;
    private float m_OriginalFixedDeltaTimeScale;
    private bool m_Paused;

    protected override void Awake()
    {
        if (!HasInstance)
        {
            Initialize();
        }

        base.Awake();
    }

    public void PauseGame()
    {
        if (m_Paused) return;

        m_Paused = true;

        m_Input.SetUI();

        m_OriginalTimeScale = Time.timeScale;
        m_OriginalFixedDeltaTimeScale = Time.fixedDeltaTime;

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;
    }

    public void ResumeGame()
    {
        if (!m_Paused) return;

        m_Paused = false;

        m_Input.SetGameplay();

        Time.timeScale = m_OriginalTimeScale;
        Time.fixedDeltaTime = m_OriginalFixedDeltaTimeScale;
    }

    public void Initialize()
    {
        m_Input = InputReader.Load();
        m_Input.SetGameplay();

        m_OriginalTimeScale = Time.timeScale;
        m_OriginalFixedDeltaTimeScale = Time.fixedDeltaTime;
    }

}