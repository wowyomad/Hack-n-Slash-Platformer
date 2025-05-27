using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheGame
{
    [RequireComponent(typeof(LevelManager))]
    public class GameManager : PersistentSingleton<GameManager>
    {
        public event Action LoadStarted;
        public event Action LoadCompleted;
        public bool IsGamePaused => m_Paused;

        [SerializeField] private InputReader m_Input;
        [SerializeField] private LevelManager m_LevelManager;

        private float m_OriginalTimeScale;
        private float m_OriginalFixedDeltaTime;
        private bool m_Paused;


        protected override void Awake()
        {
            if (!HasInstance)
            {
                Initialize();
            }

            base.Awake();
        }

        private void Start()
        {
            m_OriginalTimeScale = Time.timeScale;
            m_OriginalFixedDeltaTime = Time.fixedDeltaTime;
        }

        public void PauseGame()
        {
            if (m_Paused) return;

            m_Paused = true;

            m_Input.SetUI();

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

        public void ResetTimeScale()
        {
            Time.timeScale = m_OriginalTimeScale;
            Time.fixedDeltaTime = m_OriginalFixedDeltaTime;
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

            if (m_LevelManager == null)
            {
                m_LevelManager = GetComponent<LevelManager>();
                if (m_LevelManager == null)
                {
                    Debug.LogError("LevelManager component is missing on GameManager.");
                    return;
                }
                else
                {
                    Debug.Log("LevelManager found!");
                }
            }
        }

        public void Quit()
        {
            Debug.Log("Quitting game...");
            m_LevelManager.Save();
            Application.Quit();
        }
    }
}
