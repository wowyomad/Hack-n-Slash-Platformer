using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : PersistentSingleton<UIManager>
{
    private InputReader m_Input;
    private GameManager m_GameManager;

    [SerializeField] private GameObject m_MainScreen;
    [SerializeField] private GameObject m_PauseScreen;
    [SerializeField] private GameObject m_OptionsScreen;
    [SerializeField] private GameObject m_StartScreen;
    [SerializeField] private GameObject m_ContinueScreen;
    [SerializeField] private GameObject m_ExitScreen;
    [SerializeField] private GameObject m_DeathScreen;
    [SerializeField] private GameObject m_HUD;

    [SerializeField] private GameObject m_Settings_AudioTab;
    [SerializeField] private GameObject m_Settings_GameplayTab;
    [SerializeField] private GameObject m_Settings_KeyBindingsTab;

    private GameObject m_CurrentScreen;
    private GameObject m_CurrentSettingsTab;

    private List<GameObject> m_Screens = new List<GameObject>();
    private List<GameObject> m_SettingsTabs = new List<GameObject>();

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

    }

    private void OnEnable()
    {
        m_Input.Pause += OnPauseResume;
        m_Input.Resume += OnPauseResume;
    }

    private void OnDisable()
    {
        m_Input.Pause -= OnPauseResume;
        m_Input.Resume -= OnPauseResume;
    }

    public void OnPauseResume()
    {
        if (m_CurrentScreen == null || m_CurrentScreen == m_HUD)
        {
            ShowScreen(m_MainScreen);
            m_GameManager.PauseGame();
        }
        else if (m_CurrentScreen != m_MainScreen)
        {
            ShowScreen(m_MainScreen);
        }
        else if (m_CurrentScreen == m_MainScreen)
        {
            ShowScreen(null);
            m_GameManager.ResumeGame();
        }
    }

    public void ShowScreen(GameObject screen)
    {
        foreach (var s in m_Screens)
        {
            if (s != null)
            {
                s.SetActive(s == screen);
                m_CurrentScreen = screen;
            }
        }
        m_CurrentScreen = screen;
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
        m_Input = InputReader.Load();
        m_GameManager = GameManager.Instance;

        if (m_MainScreen == null) Debug.LogError("Main Screen is not assigned!", this);
        if (m_PauseScreen == null) Debug.LogError("Pause Screen is not assigned!", this);
        if (m_OptionsScreen == null) Debug.LogError("Options Screen is not assigned!", this);
        if (m_StartScreen == null) Debug.LogError("Start Screen is not assigned!", this);
        if (m_ContinueScreen == null) Debug.LogError("Continue Screen is not assigned!", this);
        if (m_ExitScreen == null) Debug.LogError("Exit Screen is not assigned!", this);
        if (m_DeathScreen == null) Debug.LogError("Death Screen is not assigned!", this);
        if (m_HUD == null) Debug.LogError("HUD is not assigned!", this);

        if (m_Settings_AudioTab == null) Debug.LogError("Audio Tab is not assigned!", this);
        if (m_Settings_GameplayTab == null) Debug.LogError("Gameplay Tab is not assigned!", this);
        if (m_Settings_KeyBindingsTab == null) Debug.LogError("Key Bindings Tab is not assigned!", this);

        m_Screens.Add(m_MainScreen);
        m_Screens.Add(m_PauseScreen);
        m_Screens.Add(m_OptionsScreen);
        m_Screens.Add(m_StartScreen);
        m_Screens.Add(m_ContinueScreen);
        m_Screens.Add(m_ExitScreen);
        m_Screens.Add(m_DeathScreen);
        m_Screens.Add(m_HUD);
        m_SettingsTabs.Add(m_Settings_AudioTab);
        m_SettingsTabs.Add(m_Settings_GameplayTab);
        m_SettingsTabs.Add(m_Settings_KeyBindingsTab);

        ShowScreen(null);
        ShowSettingsTab(null);
    }

}