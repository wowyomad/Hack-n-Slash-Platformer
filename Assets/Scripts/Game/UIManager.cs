using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheGame
{
    public class UIManager : MonoBehaviour
    {

        //temp ---
        public float RestartDelay = 0.75f;
        public float RestartTime = 0.0f;
        public bool IsRestarting { get; private set; } = false;
        public event Action Restarted;

        public void StartRestart()
        {
            IsRestarting = true;
        }

        public void PerformRestart()
        {
            m_LevelManager.RestartCurrentLevel();
            ShowScreen(HUD);
            ChallengePopup.HidePopup();

            Restarted?.Invoke();

            IsRestarting = false;
            RestartTime = 0.0f;
        }

        public void CancelRestart()
        {
            IsRestarting = false;
        }

        private void Update()
        {
            if (IsRestarting)
            {
                RestartTime += Time.deltaTime;
                if (RestartTime >= RestartDelay)
                {
                    PerformRestart();
                }
            }
            else if (RestartTime > 0.0f)
            {
                RestartTime = Mathf.Clamp(RestartTime, 0.0f, RestartTime - Time.deltaTime);
            }
        }
        // ----

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

        [SerializeField] public GameObject MainScreen;
        [SerializeField] public GameObject PauseScreen;
        [SerializeField] public GameObject OptionsScreen;
        [SerializeField] public GameObject StartScreen;
        [SerializeField] public GameObject ContinueScreen;
        [SerializeField] public GameObject LevelCompleteScreen;
        [SerializeField] public GameObject ExitScreen;
        [SerializeField] public GameObject DeathScreen;
        [SerializeField] public GameObject HUD;

        [SerializeField] public GameObject Settings_AudioTab;
        [SerializeField] public GameObject Settings_GameplayTab;
        [SerializeField] public GameObject Settings_KeyBindingsTab;
        [SerializeField] public ChallengePopup ChallengePopup;

        private GameObject m_CurrentScreen;
        public GameObject CurrentScreen => m_CurrentScreen;
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
                ShowScreen(MainScreen);
                m_Input.SetUI();
            }
            else
            {
                ShowScreen(null);
                m_Input.SetGameplay();
            }
            ShowSettingsTab(null);
            m_ScreenToReturnToFromOptions = m_ShowMainScreenOnStart ? MainScreen : HUD;
        }

        private void OnEnable()
        {
            m_Input.Pause += OnPauseResume;
            m_Input.Resume += OnPauseResume;
            m_Input.RestartStarted += StartRestart;
            m_Input.RestartCancelled += CancelRestart;
            m_Input.ImmediateRestart += PerformRestart;
            EventBus<PlayerDiedEvent>.OnEvent += ShowDeathScreen;
            EventBus<LevelTimeExpiredEvent>.OnEvent += ShowDeathScreen;
        }

        private void OnDisable()
        {
            m_Input.Pause -= OnPauseResume;
            m_Input.Resume -= OnPauseResume;
            m_Input.RestartStarted -= StartRestart;
            m_Input.RestartCancelled -= CancelRestart;
            m_Input.ImmediateRestart -= PerformRestart;
            EventBus<PlayerDiedEvent>.OnEvent -= ShowDeathScreen;
            EventBus<LevelTimeExpiredEvent>.OnEvent -= ShowDeathScreen;
        }

        public void OnPauseResume()
        {
            if (m_CurrentScreen == LevelCompleteScreen)
            {
                return; //ignore
            }

            if (m_CurrentScreen == null || m_CurrentScreen == HUD)
            {
                ShowScreen(PauseScreen);
                m_GameManager.PauseGame();
            }
            else if (m_CurrentScreen == PauseScreen)
            {
                ShowScreen(HUD);
                m_GameManager.ResumeGame();
            }
            else if (m_CurrentScreen == MainScreen)
            {
                m_GameManager.Quit();
            }
            else if (m_CurrentScreen == OptionsScreen)
            {
                GoBackFromOptions();
            }
            else
            {
                if (m_GameManager.IsGamePaused)
                {
                    ShowScreen(PauseScreen);
                }
                else
                {
                    ShowScreen(MainScreen);
                }
            }
        }

        public void DisplayChallengePopup(Challenge challenge, bool success)
        {
            ChallengePopup.ShowPopup(challenge, success);
        }
        public void HideChallengePopup()
        {
            ChallengePopup.HidePopup();
        }

        public void ShowScreen(ScreenType screenType)
        {
            GameObject screen = GetScreenObject(screenType);
            if (screen != null)
            {
                ShowScreen(screen);
            }
            else
            {
                Debug.LogError($"Screen of type {screenType} not found!", this);
            }
        }
        public void ShowScreen(GameObject screen)
        {
            GameObject previousScreen = m_CurrentScreen;

            if (previousScreen == LevelCompleteScreen && screen == PauseScreen)
            {
                return;
            }

            ScreenType previousScreenType = GetScreenType(previousScreen);

            if (screen == OptionsScreen && previousScreen != OptionsScreen)
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
                    ShowScreen(PauseScreen);
                }
                else
                {
                    ShowScreen(MainScreen);
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

            if (MainScreen == null) Debug.LogError("Main Screen is not assigned!", this);
            if (PauseScreen == null) Debug.LogError("Pause Screen is not assigned!", this);
            if (OptionsScreen == null) Debug.LogError("Options Screen is not assigned!", this);
            if (StartScreen == null) Debug.LogError("Start Screen is not assigned!", this);
            if (ContinueScreen == null) Debug.LogError("Continue Screen is not assigned!", this);
            if (LevelCompleteScreen == null) Debug.LogError("Level Complete Screen is not assigned!", this);
            if (ExitScreen == null) Debug.LogError("Exit Screen is not assigned!", this);
            if (DeathScreen == null) Debug.LogError("Death Screen is not assigned!", this);
            if (HUD == null) Debug.LogError("HUD is not assigned!", this);

            if (Settings_AudioTab == null) Debug.LogError("Audio Tab is not assigned!", this);
            if (Settings_GameplayTab == null) Debug.LogError("Gameplay Tab is not assigned!", this);
            if (Settings_KeyBindingsTab == null) Debug.LogError("Key Bindings Tab is not assigned!", this);

            m_Screens.Add(MainScreen);
            m_Screens.Add(PauseScreen);
            m_Screens.Add(OptionsScreen);
            m_Screens.Add(StartScreen);
            m_Screens.Add(ContinueScreen);
            m_Screens.Add(LevelCompleteScreen);
            m_Screens.Add(ExitScreen);
            m_Screens.Add(DeathScreen);
            m_Screens.Add(HUD);
            m_SettingsTabs.Add(Settings_AudioTab);
            m_SettingsTabs.Add(Settings_GameplayTab);
            m_SettingsTabs.Add(Settings_KeyBindingsTab);

            m_Screens.ForEach(screen =>
            {
                if (screen != null)
                {
                    screen.SetActive(true); //trigger Awake
                    screen.SetActive(false);
                }
            });
            m_SettingsTabs.ForEach(tab =>
            {
                if (tab != null)
                {
                    tab.SetActive(true);
                    tab.SetActive(false);
                }
            });
        }




        private void ShowDeathScreen(PlayerDiedEvent e)
        {
            m_Input.SetDeath();
            ShowScreen(DeathScreen);
        }

        private void ShowDeathScreen(LevelTimeExpiredEvent e)
        {
            m_Input.SetDeath();
            ShowScreen(DeathScreen);
        }

        public ScreenType GetScreenType(GameObject screen)
        {
            if (screen == MainScreen) return ScreenType.MainScreen;
            if (screen == PauseScreen) return ScreenType.PauseScreen;
            if (screen == OptionsScreen) return ScreenType.OptionsScreen;
            if (screen == StartScreen) return ScreenType.StartScreen;
            if (screen == ContinueScreen) return ScreenType.ContinueScreen;
            if (screen == LevelCompleteScreen) return ScreenType.LevelCompleteScreen;
            if (screen == ExitScreen) return ScreenType.ExitScreen;
            if (screen == DeathScreen) return ScreenType.DeathScreen;
            if (screen == HUD) return ScreenType.HUD;

            return ScreenType.None;
        }

        public GameObject GetScreenObject(ScreenType screenType)
        {
            switch (screenType)
            {
            case ScreenType.MainScreen: return MainScreen;
            case ScreenType.PauseScreen: return PauseScreen;
            case ScreenType.OptionsScreen: return OptionsScreen;
            case ScreenType.StartScreen: return StartScreen;
            case ScreenType.ContinueScreen: return ContinueScreen;
            case ScreenType.LevelCompleteScreen: return LevelCompleteScreen;
            case ScreenType.ExitScreen: return ExitScreen;
            case ScreenType.DeathScreen: return DeathScreen;
            case ScreenType.HUD: return HUD;
            default: return null;
            }
        }
    }
}