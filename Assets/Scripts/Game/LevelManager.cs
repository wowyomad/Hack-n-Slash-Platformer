using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject m_MainMenu;
    [SerializeField] protected InputReader Input;

    private GameManager m_GameManager;

    private void Awake()
    {
        m_GameManager = GameManager.Instance;
        Input = InputReader.Load();
        if (m_MainMenu == null)
        {
            Debug.LogError("Main Menu is not assigned in the inspector.");
        }
    }

    private void OnEnable()
    {
        Input.Pause += ShowMenu;
        Input.Resume += HideMenu;
    }

    private void OnDisable()
    {
        Input.Pause -= ShowMenu;
        Input.Resume -= HideMenu;
    }

    private void HideMenu()
    {
        Debug.Log("Hiding menu");
        Input.SetGameplay();
        m_GameManager.ResumeGame();
        m_MainMenu.SetActive(false);
    }

    private void ShowMenu()
    {
        Debug.Log("Showing menu");

        Input.SetUI();
        m_GameManager.PauseGame();
        m_MainMenu.SetActive(true);
    }
}
