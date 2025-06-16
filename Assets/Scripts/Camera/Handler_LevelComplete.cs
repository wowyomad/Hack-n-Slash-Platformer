using TheGame;
using TMPro;
using UnityEngine;



public class Handler_LevelComplete : MonoBehaviour
{
    [SerializeField] private GameObject m_ContentReference;
    [SerializeField] private GameObject m_ChallengePrefab;
    private GameManager m_GameManager;
    private LevelManager m_LevelManager;
    private UIManager m_UIManager;
    private Handler_StartMenu m_StartHandler;

    

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

    private void ShowLevelCompleteScreen(LevelCompletedEvent e)
    {
        m_CurrentLevel = e.Level;

        m_GameManager.PauseGame();
        m_UIManager.ShowScreen(m_UIManager.LevelCompleteScreen);

        //TODO: this is only called here because for some reason this piece of shit sometimes is triggered multiple times per level completion
        ClearChallenges();

        //Instantiate challenge prefab with currentlevel information
        m_CurrentLevel.Challenges.ForEach(challenge =>
        {
            GameObject challengeObj = Instantiate(m_ChallengePrefab, m_ContentReference.transform);
            var description = challengeObj.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            description.text = challenge.Description;
            var themeApplier = description.GetComponent<TextThemeApplier>();

            if (challenge.Status == ChallengeStatus.Complete)
            {
                description.fontStyle = FontStyles.Strikethrough;
                themeApplier.ColorType = TextThemeApplier.TextColorType.GoodGlpyhs;
                themeApplier.ApplyTheme();
            }
            else
            {
                description.fontStyle = FontStyles.Normal;
                themeApplier.ColorType = TextThemeApplier.TextColorType.BadGlyphs;
                themeApplier.ApplyTheme();
            }
        });
    }

    public void ClearChallenges()
    {
        foreach (Transform child in m_ContentReference.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void GoToMenu()
    {
        m_StartHandler.RefreshState();
        m_LevelManager.UnloadCurrentLevel();
        m_UIManager.ShowScreen(m_UIManager.MainScreen);

        ClearChallenges();
    }

    public void GoNext()
    {

        if (m_CurrentLevel.NextLevels.Count > 0)
        {
            m_LevelManager.UnloadCurrentLevel();
            m_LevelManager.LoadLevel(m_CurrentLevel.NextLevels[0]);
            m_UIManager.ShowScreen(m_UIManager.HUD);
        }
        else
        {
            Debug.LogWarning("No next level available. Returning to main menu.");
            GoToMenu();
        }

        ClearChallenges();
    }
}
