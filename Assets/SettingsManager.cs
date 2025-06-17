using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Settings Keys")]
    [SerializeField] private string MusicVolumeKey = "MusicVolume";
    [SerializeField] private string GeneralVolumeKey = "GeneralVolume";

    [Header("UI Controllers")]
    [SerializeField] private Slider GeneralVolumeSlider;
    [SerializeField] private Slider MusicVolumeSlider;


    private AudioManager m_AudioManager;


    [SerializeField] private DefaultSettings m_DefaultSettings;

    private InputReader Input;

    private void Awake()
    {
        Input = Resources.Load<InputReader>("Input/InputReader");
        m_AudioManager = GetComponent<AudioManager>();

        if (m_DefaultSettings == null)
        {
            Debug.LogError("DefaultSettingsSO is not assigned.");
        }
    }

    private void Start()
    {
        LoadSettings();
        GeneralVolumeSlider.value = m_AudioManager.GeneralVolume;
        MusicVolumeSlider.value = m_AudioManager.MusicVolume;
    }

    private void OnEnable()
    {
        GeneralVolumeSlider.onValueChanged.AddListener(SetGeneralVolume);
        MusicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    private void OnDisable()
    {
        GeneralVolumeSlider.onValueChanged.RemoveListener(SetGeneralVolume);
        MusicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
    }

    public void SetMusicVolume(float volume)
    {
        m_AudioManager.SetMusicVolume(volume);
    }

    public void SetGeneralVolume(float volume)
    {
        m_AudioManager.SetGeneralVolume(volume);
    }

    public void LoadSettings()
    {
        RestoreSettings();
    }


    public void RestoreSettings()
    {
        m_AudioManager.SetGeneralVolume(PlayerPrefs.GetFloat(GeneralVolumeKey, m_DefaultSettings.GeneralVolume));
        m_AudioManager.SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey, m_DefaultSettings.MusicVolume));
    }


    public void ApplySettings()
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, m_AudioManager.MusicVolume);
        PlayerPrefs.SetFloat(GeneralVolumeKey, m_AudioManager.GeneralVolume);
        PlayerPrefs.Save();
    }

    public void ResetSettings()
    {
        m_AudioManager.SetGeneralVolume(m_DefaultSettings.GeneralVolume);
        m_AudioManager.SetMusicVolume(m_DefaultSettings.MusicVolume);
    }

    private void OnApplicationQuit()
    {
        ApplySettings();
    }
}
