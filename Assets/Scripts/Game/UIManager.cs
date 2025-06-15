using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheGame
{
    public class UIManager : MonoBehaviour
    {
        public event Action<ScreenType, ScreenType> OnScreenChanged;
        public enum ScreenType
        {
            None,
            MainScreen,
            PauseScreen,
            OptionsScreen,
            StartScreen,
            ContinueScreen,
            LevelCompleteScreen,
            ExitScreen,
            DeathScreen,
            HUD
        }
        [SerializeField] private InputReader m_Input;
        private GameManager m_GameManager;

        [SerializeField] private bool m_ShowMainScreenOnStart = true;

        [SerializeField] public GameObject m_MainScreen;
        [SerializeField] public GameObject m_PauseScreen;
        [SerializeField] public GameObject m_OptionsScreen;
        [SerializeField] public GameObject m_StartScreen;
        [SerializeField] public GameObject m_ContinueScreen;
        [SerializeField] public GameObject LevelCompleteScreen;
        [SerializeField] public GameObject m_ExitScreen;
        [SerializeField] public GameObject m_DeathScreen;
        [SerializeField] public GameObject HUD;

        [SerializeField] private GameObject m_Settings_AudioTab;
        [SerializeField] private GameObject m_Settings_GameplayTab;
        [SerializeField] private GameObject m_Settings_KeyBindingsTab;

        private GameObject m_CurrentScreen;
        private GameObject m_CurrentSettingsTab;
        private LevelManager m_LevelManager;
        private GameObject m_ScreenToReturnToFromOptions;

        private List<GameObject> m_Screens = new List<GameObject>();
        private List<GameObject> m_SettingsTabs = new List<GameObject>();

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            if (m_ShowMainScreenOnStart)
            {
                ShowScreen(m_MainScreen);
                m_Input.SetUI();
            }
            else
            {
                ShowScreen(null);
                m_Input.SetGameplay();
            }
            ShowSettingsTab(null);
            m_ScreenToReturnToFromOptions = m_ShowMainScreenOnStart ? m_MainScreen : HUD;
        }

        private void OnEnable()
        {
            m_Input.Pause += OnPauseResume;
            m_Input.Resume += OnPauseResume;
            m_Input.Restart += Restart;
            EventBus<PlayerDiedEvent>.OnEvent += ShowDeathScreen;
            EventBus<LevelTimeExpiredEvent>.OnEvent += ShowDeathScreen;
        }

        private void OnDisable()
        {
            m_Input.Pause -= OnPauseResume;
            m_Input.Resume -= OnPauseResume;
            m_Input.Restart -= Restart;
            EventBus<PlayerDiedEvent>.OnEvent -= ShowDeathScreen;
            EventBus<LevelTimeExpiredEvent>.OnEvent -= ShowDeathScreen;
        }

        public void OnPauseResume()
        {
            if (m_CurrentScreen == null || m_CurrentScreen == HUD)
            {
                ShowScreen(m_PauseScreen);
                m_GameManager.PauseGame();
            }
            else if (m_CurrentScreen == m_PauseScreen)
            {
                ShowScreen(HUD);
                m_GameManager.ResumeGame();
            }
            else if (m_CurrentScreen == m_MainScreen)
            {
                m_GameManager.Quit();
            }
            else if (m_CurrentScreen == m_OptionsScreen)
            {
                GoBackFromOptions();
            }
            else
            {
                if (m_GameManager.IsGamePaused)
                {
                    ShowScreen(m_PauseScreen);
                }
                else
                {
                    ShowScreen(m_MainScreen);
                }
            }
        }

        public void ShowScreen(GameObject screen)
        {
            GameObject previousScreen = m_CurrentScreen;
            ScreenType previousScreenType = GetScreenType(previousScreen);

            if (screen == m_OptionsScreen && previousScreen != m_OptionsScreen)
            {
                m_ScreenToReturnToFromOptions = previousScreen;
            }

            foreach (var s in m_Screens)
            {
                if (s != null)
                {
                    s.SetActive(s == screen);
                }
            }
            m_CurrentScreen = screen;

            ScreenType currentScreenType = GetScreenType(screen);
            if (previousScreenType != currentScreenType)
            {
                OnScreenChanged?.Invoke(previousScreenType, currentScreenType);
            }
        }

        public void GoBackFromOptions()
        {
            if (m_ScreenToReturnToFromOptions != null)
            {
                ShowScreen(m_ScreenToReturnToFromOptions);
            }
            else
            {
                if (m_GameManager != null && m_GameManager.IsGamePaused)
                {
                    ShowScreen(m_PauseScreen);
                }
                else
                {
                    ShowScreen(m_MainScreen);
                }
            }
        }

        public void HideAllScreens()
        {
            ShowScreen(null);
        }

        public void ShowSettingsTab(GameObject tab)
        {
            foreach (var t in m_SettingsTabs)
            {
                if (t != null)
                {
                    t.SetActive(t == tab);
                }
            }
            m_CurrentSettingsTab = tab;
        }
        private void Initialize()
        {
            if (m_Input == null)
            {
                m_Input = InputReader.Load();
            }
            m_GameManager = GetComponent<GameManager>();
            m_LevelManager = GetComponent<LevelManager>();

            if (m_MainScreen == null) Debug.LogError("Main Screen is not assigned!", this);
            if (m_PauseScreen == null) Debug.LogError("Pause Screen is not assigned!", this);
            if (m_OptionsScreen == null) Debug.LogError("Options Screen is not assigned!", this);
            if (m_StartScreen == null) Debug.LogError("Start Screen is not assigned!", this);
            if (m_ContinueScreen == null) Debug.LogError("Continue Screen is not assigned!", this);
            if (LevelCompleteScreen == null) Debug.LogError("Level Complete Screen is not assigned!", this);
            if (m_ExitScreen == null) Debug.LogError("Exit Screen is not assigned!", this);
            if (m_DeathScreen == null) Debug.LogError("Death Screen is not assigned!", this);
            if (HUD == null) Debug.LogError("HUD is not assigned!", this);

            if (m_Settings_AudioTab == null) Debug.LogError("Audio Tab is not assigned!", this);
            if (m_Settings_GameplayTab == null) Debug.LogError("Gameplay Tab is not assigned!", this);
            if (m_Settings_KeyBindingsTab == null) Debug.LogError("Key Bindings Tab is not assigned!", this);

            m_Screens.Add(m_MainScreen);
            m_Screens.Add(m_PauseScreen);
            m_Screens.Add(m_OptionsScreen);
            m_Screens.Add(m_StartScreen);
            m_Screens.Add(m_ContinueScreen);
            m_Screens.Add(LevelCompleteScreen);
            m_Screens.Add(m_ExitScreen);
            m_Screens.Add(m_DeathScreen);
            m_Screens.Add(HUD);
            m_SettingsTabs.Add(m_Settings_AudioTab);
            m_SettingsTabs.Add(m_Settings_GameplayTab);
            m_SettingsTabs.Add(m_Settings_KeyBindingsTab);
        }


        public void Restart()
        {
            m_LevelManager.RestartCurrentLevel();
            ShowScreen(HUD);
        }

        private void ShowDeathScreen(PlayerDiedEvent e)
        {
            m_Input.SetDeath();
            ShowScreen(m_DeathScreen);
        }

        private void ShowDeathScreen(LevelTimeExpiredEvent e)
        {
            m_Input.SetDeath();
            ShowScreen(m_DeathScreen);
        }

        private ScreenType GetScreenType(GameObject screen)
        {
            if (screen == m_MainScreen) return ScreenType.MainScreen;
            if (screen == m_PauseScreen) return ScreenType.PauseScreen;
            if (screen == m_OptionsScreen) return ScreenType.OptionsScreen;
            if (screen == m_StartScreen) return ScreenType.StartScreen;
            if (screen == m_ContinueScreen) return ScreenType.ContinueScreen;
            if (screen == LevelCompleteScreen) return ScreenType.LevelCompleteScreen;
            if (screen == m_ExitScreen) return ScreenType.ExitScreen;
            if (screen == m_DeathScreen) return ScreenType.DeathScreen;
            if (screen == HUD) return ScreenType.HUD;

            return ScreenType.None;
        }
    }
}