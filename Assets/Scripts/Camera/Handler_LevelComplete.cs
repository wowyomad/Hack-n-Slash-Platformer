using TheGame;
using TMPro;
using UnityEngine;



public class Handler_LevelComplete : MonoBehaviour
{
    private GameManager m_GameManager;
    private LevelManager m_LevelManager;
    private UIManager m_UIManager;
    private Handler_StartMenu m_StartHandler;

    public TextMeshProUGUI DescriptionText;

    public InputReader m_InputReader;

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
    private bool m_Green = false;
    private void OnEnable()
    {
        EventBus<LevelCompletedEvent>.OnEvent += ShowLevelCompleteScreen;
    }

    private void OnDisable()
    {
        EventBus<LevelCompletedEvent>.OnEvent -= ShowLevelCompleteScreen;
    }
    void Start()
    {

    }

    private void ShowLevelCompleteScreen(LevelCompletedEvent e)
    {
        m_CurrentLevel = e.Level;

        m_GameManager.PauseGame();
        m_UIManager.ShowScreen(m_UIManager.LevelCompleteScreen);
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
