using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheGame
{
    [RequireComponent(typeof(LevelManager))]
    public class GameManager : MonoBehaviour
    {
        public event Action LoadStarted;
        public event Action LoadCompleted;
        public event Action UnloadCompleted;
        public bool IsGamePaused => m_Paused;

        [SerializeField] private InputReader m_Input;
        private LevelManager m_LevelManager; // Assigned via GetComponent in Initialize

        private float m_OriginalTimeScale = 1.0f;
        private float m_OriginalFixedDeltaTime = 0.02f; // Default Unity value
        private bool m_Paused;
        private bool m_TimeScalesCaptured = false;

        private string m_CurrentAdditiveGameplaySceneName; // Tracks the current game level scene

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            if (!m_TimeScalesCaptured)
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

        public void UnloadGameLevel(string levelSceneName) // Renamed for clarity
        {
            if (string.IsNullOrEmpty(levelSceneName) || !SceneManager.GetSceneByName(levelSceneName).isLoaded) return;
            StartCoroutine(UnloadSceneCoroutine(levelSceneName));
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
            UnloadCompleted?.Invoke();
        }

        private void SetupCamera()
        {
            CameraBootstrap cameraBootstrap = FindFirstObjectByType<CameraBootstrap>();
            cameraBootstrap?.Initialize();
        }
        public void Quit()
        {
            m_LevelManager?.Save();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}