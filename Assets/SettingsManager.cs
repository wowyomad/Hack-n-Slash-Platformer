using UnityEngine;

public class DefaultSettingsSO : ScriptableObject
{
    public float MusicVolume = 0.5f;
    public float GeneralVolume = 0.75f;
}

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private AudioSource m_MusicSource;
    [SerializeField] private string MusicVolumeKey = "MusicVolume";
    [SerializeField] private string GeneralVolumeKey = "GeneralVolume";


    [SerializeField] private DefaultSettingsSO m_DefaultSettings;

    private InputReader Input;

    private void Awake()
    {
        Input = Resources.Load<InputReader>("Input/InputReader");

        if (m_DefaultSettings == null)
        {
            Debug.LogError("DefaultSettingsSO is not assigned.");
        }
    }

    public void SetMusicVolume(float volume)
    {
        m_MusicSource.volume = volume;
    }

    public void SetGeneralVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void LoadSettings()
    {
        RestoreSettings();
    }


    public void RestoreSettings()
    {
        m_MusicSource.volume = PlayerPrefs.GetFloat(MusicVolumeKey, m_DefaultSettings.MusicVolume);
        AudioListener.volume = PlayerPrefs.GetFloat(GeneralVolumeKey, m_DefaultSettings.GeneralVolume);
    }


    public void ApplySettings()
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, m_MusicSource.volume);
        PlayerPrefs.SetFloat(GeneralVolumeKey, AudioListener.volume);
        PlayerPrefs.Save();
    }

    public void ResetSettings()
    {
        AudioListener.volume = m_DefaultSettings.GeneralVolume;
        m_MusicSource.volume = m_DefaultSettings.MusicVolume;
    }
}
