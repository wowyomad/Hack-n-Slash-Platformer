using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : PersistentSingleton<GameManager>
{

    public bool IsGamePaused => m_Paused;

    [SerializeField] private InputReader m_Input;

    private float m_OriginalTimeScale = 1.0f;
    private float m_OriginalFixedDeltaTime = 0.02f;
    private bool m_Paused;

    public event Action LoadStarted;
    public event Action LoadCompleted;

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
        m_OriginalFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;
    }

    public void ResumeGame()
    {
        if (!m_Paused) return;

        m_Paused = false;

        m_Input.SetGameplay();

        Time.timeScale = m_OriginalTimeScale;
        Time.fixedDeltaTime = m_OriginalFixedDeltaTime;
    }


    public void RestartGame()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadScene(string name)
    {
        StartCoroutine(LoadSceneCoroutine(name));
    }

    private System.Collections.IEnumerator LoadSceneCoroutine(string name)
    {
        m_Input.SetGameplay();

        LoadStarted?.Invoke();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        LoadCompleted?.Invoke();
    }

    public void UnloadScene(string name)
    {
        StartCoroutine(UnloadSceneCoroutine(name));
    }

    private System.Collections.IEnumerator UnloadSceneCoroutine(string name)
    {
        m_Input.SetGameplay();

        LoadStarted?.Invoke();

        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(name);
        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        LoadCompleted?.Invoke();
    }

    public void Initialize()
    {
        if (m_Input == null)
        {
            m_Input = InputReader.Load();
        }

        m_OriginalTimeScale = Time.timeScale;
        m_OriginalFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void Quit()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}