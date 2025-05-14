using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager m_Instance;
    public static GameManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindFirstObjectByType<GameManager>();
                if (m_Instance == null)
                {
                    GameObject obj = new GameObject("GameManager Auto-Generated");
                    m_Instance = obj.AddComponent<GameManager>();
                }
            }
            return m_Instance;
        }
    }

    public bool IsGamePaused => m_Paused;

    private float m_OriginalTimeScale;
    private float m_OriginalFixedDeltaTimeScale;
    private bool m_Paused;

    public void PauseGame()
    {
        if (m_Paused) return;

        m_Paused = true;

        m_OriginalTimeScale = Time.timeScale;
        m_OriginalFixedDeltaTimeScale = Time.fixedDeltaTime;

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;
    }

    public void ResumeGame()
    {
        if (!m_Paused) return;

        m_Paused = false;

        Time.timeScale = m_OriginalTimeScale;
        Time.fixedDeltaTime = m_OriginalFixedDeltaTimeScale;
    }

    private void Awake()
    {
        if (m_Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        m_Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (m_Instance == this)
        {
            m_Instance = null;
        }
    }
}