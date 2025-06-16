using TheGame;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public float MusicVolume => m_MusicSource != null ? m_MusicSource.volume : 0f;
    public float GeneralVolume => AudioListener.volume;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource m_MusicSource;
    [SerializeField] private AudioSource m_SFXSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip m_MenuMusic;
    [SerializeField] private AudioClip m_LevelMusic;
    [SerializeField] private AudioClip m_WinClip;
    [SerializeField] private AudioClip m_LoseClip;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip m_ButtonClickSFX;
    [SerializeField] private AudioClip m_EnemyHitSound;

    private UIManager m_UIManager;
    private LevelManager m_LevelManager;

    private void Awake()
    {
        m_UIManager = GetComponent<UIManager>();
        if (m_UIManager != null)
        {
            m_UIManager.OnScreenChanged += HandleScreenChanged;
        }
        else
        {
            Debug.LogError("UIManager component not found on AudioManager GameObject.", this);
        }


        m_LevelManager = GetComponent<LevelManager>();
    }

    private void Start()
    {
        
    }

    private void OnDestroy()
    {
        if (m_UIManager != null)
        {
            m_UIManager.OnScreenChanged -= HandleScreenChanged;
        }
    }

    public void SetGeneralVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }

    public void SetMusicVolume(float volume)
    {
        m_MusicSource.volume = Mathf.Clamp01(volume);
    }

    private void HandleScreenChanged(UIManager.ScreenType previousScreen, UIManager.ScreenType newScreen)
    {
        if (m_MusicSource == null)
        {
            Debug.LogError("MusicSource is not assigned in AudioManager.", this);
            return;
        }

        switch (newScreen)
        {
            case UIManager.ScreenType.MainScreen:
            case UIManager.ScreenType.StartScreen:
                PlayMusic(m_MenuMusic, true);
                break;

            case UIManager.ScreenType.HUD:
                if (previousScreen == UIManager.ScreenType.PauseScreen &&
                    m_MusicSource.clip == m_LevelMusic &&
                    !m_MusicSource.isPlaying)
                {
                    m_MusicSource.UnPause();
                }
                else
                {
                    PlayMusic(m_LevelMusic, true);
                }
                break;

            case UIManager.ScreenType.PauseScreen:
                break;

            case UIManager.ScreenType.DeathScreen:
                PlayMusic(m_LoseClip, false);
                break;

            case UIManager.ScreenType.LevelCompleteScreen:
                PlayMusic(m_WinClip, false);
                break;

            default:
                break;
        }
    }

    public void PlayMusic(AudioClip clip, bool loop)
    {
        if (m_MusicSource == null) return;

        if (clip != null)
        {
            if (m_MusicSource.clip == clip && m_MusicSource.isPlaying && m_MusicSource.loop == loop)
            {
                return;
            }

            m_MusicSource.Stop();
            m_MusicSource.clip = clip;
            m_MusicSource.loop = loop;
            m_MusicSource.Play();
        }
        else
        {
            m_MusicSource.Stop();
        }
    }

    public void PlayButtonClickSFX()
    {
        if (m_SFXSource != null && m_ButtonClickSFX != null)
        {
            m_SFXSource.PlayOneShot(m_ButtonClickSFX);
        }
    }

    public void PlayEnemyHitSound()
    {
        if (m_SFXSource != null && m_EnemyHitSound != null)
        {
            m_SFXSource.PlayOneShot(m_EnemyHitSound);
        }
    }
}