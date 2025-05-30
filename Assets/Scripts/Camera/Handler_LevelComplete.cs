using TheGame;
using UnityEngine;

public class Handler_LevelComplete : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameManager m_GameManager;
    private LevelManager m_LevelManager;
    private UIManager m_UIManager;
    private Handler_StartMenu m_StartHandler;

    private Level m_CurrentLevel;

    private void Awake()
    {
        m_UIManager = GetComponent<UIManager>();
        m_GameManager = GetComponent<GameManager>();
        m_LevelManager = GetComponent<LevelManager>();
        m_StartHandler = GetComponent<Handler_StartMenu>();
        if (m_UIManager == null)
        {
            Debug.LogError("UIManager component is not found on the Handler_LevelComplete GameObject.", this);
        }
    }

    private void OnEnable()
    {
        EventBus<LevelCompletedEvent>.OnEvent += ShowLevelCompleteScreen;
    }
    void Start()
    {

    }



    private void ShowLevelCompleteScreen(LevelCompletedEvent e)
    {
        Level level = e.Level;

        m_GameManager.PauseGame();
        m_UIManager.ShowScreen(m_UIManager.LevelCompleteScreen);

        m_CurrentLevel = level;
    }

    public void GoToMenu()
    {
        m_StartHandler.RefreshState();
        m_LevelManager.UnloadCurrentLevel();
    }

    public void GoNext()
    {

        if (m_CurrentLevel.NextLevels.Count > 0)
        {
            m_LevelManager.UnloadCurrentLevel();
            m_LevelManager.LoadLevel(m_CurrentLevel.NextLevels[0]);
        }
        else
        {
            Debug.LogWarning("No next level available. Returning to main menu.");
            GoToMenu();
        }
    }
}
