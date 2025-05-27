using System;
using System.Collections;
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
        private LevelManager m_LevelManager; // Assigned via GetComponent in Initialize

        private float m_OriginalTimeScale = 1.0f;
        private float m_OriginalFixedDeltaTime = 0.02f; // Default Unity value
        private bool m_Paused;
        private bool m_TimeScalesCaptured = false;

        private string m_CurrentAdditiveGameplaySceneName; // Tracks the current game level scene

        protected override void Awake()
        {
            base.Awake(); // Handle singleton instance logic
            if (Instance == this) // Ensure this is the true singleton instance
            {
                Initialize();
            }
        }

        private void Start()
        {
            if (Instance == this && !m_TimeScalesCaptured)
            {
                CaptureOriginalTimeScales();
            }
        }

        private void CaptureOriginalTimeScales()
        {
            m_OriginalTimeScale = Time.timeScale;
            m_OriginalFixedDeltaTime = Time.fixedDeltaTime;
            m_TimeScalesCaptured = true;
            if (m_OriginalTimeScale == 0f && Application.isPlaying && !m_Paused)
            {
                Time.timeScale = 1.0f;
                m_OriginalTimeScale = Time.timeScale;
            }
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
                    Debug.LogError("GameManager: LevelManager component is MISSING on this GameObject.");
                }
            }

            LoadCompleted += SetupCamera;
        }

        public void PauseGame()
        {
            if (m_Paused) return;
            if (!m_TimeScalesCaptured) CaptureOriginalTimeScales();
            m_Paused = true;
            m_Input?.SetUI();
            Time.timeScale = 0f;
            // Time.fixedDeltaTime = 0f; // Generally not needed
        }

        public void ResumeGame()
        {
            if (!m_Paused) return;
            if (!m_TimeScalesCaptured) CaptureOriginalTimeScales(); // Should have been captured by Pause or Start
            m_Paused = false;
            m_Input?.SetGameplay();
            Time.timeScale = m_OriginalTimeScale;
            Time.fixedDeltaTime = m_OriginalFixedDeltaTime;
        }

        public void TriggerRestartCurrentLevel()
        {
            if (true || m_LevelManager != null /*&& m_LevelManager.IsLevelActive*/ && !string.IsNullOrEmpty(m_CurrentAdditiveGameplaySceneName))
            {
                //m_LevelManager.RestartCurrentLevel(); // Logic to reset level state (e.g. player pos, challenge status on runtime SO copies)
                LoadGameLevel(m_CurrentAdditiveGameplaySceneName); // Reload the same scene
            }
            else
            {
                Debug.LogWarning("GameManager: No active game level to restart or LevelManager not found. Consider going to main menu.");
                //m_LevelManager?.GoToMainMenu(); // Example: LevelManager handles loading main menu
            }
        }

        public void ResetTimeScale()
        {
            if (!m_TimeScalesCaptured) CaptureOriginalTimeScales();
            Time.timeScale = m_OriginalTimeScale;
            Time.fixedDeltaTime = m_OriginalFixedDeltaTime;
        }

        public void LoadSceneAdditive(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            StartCoroutine(LoadSceneAdditiveCoroutine(sceneName));
        }

        private IEnumerator LoadSceneAdditiveCoroutine(string sceneName)
        {
            LoadStarted?.Invoke();
            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
            LoadCompleted?.Invoke();
        }

        public void LoadGameLevel(string newGameLevelSceneName)
        {
            if (string.IsNullOrEmpty(newGameLevelSceneName)) return;
            StartCoroutine(LoadGameLevelCoroutine(newGameLevelSceneName));
        }

        private IEnumerator LoadGameLevelCoroutine(string newGameLevelSceneName)
        {
            m_Input?.SetGameplay();
            LoadStarted?.Invoke();

            if (!string.IsNullOrEmpty(m_CurrentAdditiveGameplaySceneName) && SceneManager.GetSceneByName(m_CurrentAdditiveGameplaySceneName).isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(m_CurrentAdditiveGameplaySceneName);
            }

            if (!SceneManager.GetSceneByName(newGameLevelSceneName).isLoaded)
            {
                yield return SceneManager.LoadSceneAsync(newGameLevelSceneName, LoadSceneMode.Additive);
            }
            m_CurrentAdditiveGameplaySceneName = newGameLevelSceneName;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(newGameLevelSceneName));

            LoadCompleted?.Invoke();
        }

        public void UnloadSpecificScene(string sceneName) // Renamed for clarity
        {
            if (string.IsNullOrEmpty(sceneName) || !SceneManager.GetSceneByName(sceneName).isLoaded) return;
            StartCoroutine(UnloadSceneCoroutine(sceneName));
        }

        private IEnumerator UnloadSceneCoroutine(string sceneName)
        {
            // Consider input state changes if needed before/after unload
            LoadStarted?.Invoke(); // Or specific "UnloadStarted" event
            yield return SceneManager.UnloadSceneAsync(sceneName);
            if (m_CurrentAdditiveGameplaySceneName == sceneName)
            {
                m_CurrentAdditiveGameplaySceneName = null;
            }
            LoadCompleted?.Invoke(); // Or specific "UnloadCompleted" event
        }

        private void SetupCamera()
        {
            CameraBootstrap cameraBootstrap = FindFirstObjectByType<CameraBootstrap>();
            cameraBootstrap?.Initialize();
        }
        public void Quit()
        {
            if (m_LevelManager == null && LevelManager.Instance != null) m_LevelManager = LevelManager.Instance;
            m_LevelManager?.Save();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}