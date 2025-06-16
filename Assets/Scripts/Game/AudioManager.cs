using TheGame;
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public float MusicVolume => m_MusicSource != null ? m_MusicSource.volume : 0f;
    public float GeneralVolume => AudioListener.volume;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource m_MusicSource;
    [SerializeField] private AudioSource m_SFXSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip m_MenuMusic;
    [SerializeField] private List<AudioClip> m_LevelMusicPlaylist;
    [SerializeField] private AudioClip m_WinClip;
    [SerializeField] private AudioClip m_LoseClip;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip m_ButtonClickSFX;
    [SerializeField] private AudioClip m_EnemyHitSound;

    private UIManager m_UIManager;
    private LevelManager m_LevelManager;

    private int m_CurrentLevelMusicIndex = 0;
    private bool m_IsPlayingLevelPlaylist = false;

    private float m_BaseMusicVolume = 1.0f;
    private bool m_IsMusicVolumeHalvedForPause = false;

    private void Awake()
    {
    }

    private void Start()
    {
        m_UIManager = GetComponent<UIManager>();
        if (m_UIManager != null)
        {
            m_UIManager.OnScreenChanged += HandleScreenChanged;
        }
        else
        {
            Debug.LogError("UIManager component not found. AudioManager may not function correctly.", this);
        }

        m_LevelManager = GetComponent<LevelManager>();
        ApplyMusicVolume();
    }

    private void Update()
    {
        if (m_IsPlayingLevelPlaylist && m_MusicSource != null && m_MusicSource.clip != null && !m_MusicSource.isPlaying)
        {
            m_CurrentLevelMusicIndex++;
            PlayNextLevelTrack();
        }
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
        m_BaseMusicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume();
    }

    private void ApplyMusicVolume()
    {
        if (m_MusicSource == null) return;

        if (m_IsMusicVolumeHalvedForPause)
        {
            m_MusicSource.volume = m_BaseMusicVolume * 0.5f;
        }
        else
        {
            m_MusicSource.volume = m_BaseMusicVolume;
        }
    }

    private void HandleScreenChanged(UIManager.ScreenType previousScreen, UIManager.ScreenType newScreen)
    {
        if (m_MusicSource == null)
        {
            Debug.LogError("MusicSource is not assigned in AudioManager.", this);
            return;
        }

        if (previousScreen == UIManager.ScreenType.PauseScreen && newScreen != UIManager.ScreenType.PauseScreen)
        {
            m_IsMusicVolumeHalvedForPause = false;
        }

        switch (newScreen)
        {
            case UIManager.ScreenType.MainScreen:
            case UIManager.ScreenType.StartScreen:
                m_IsPlayingLevelPlaylist = false;
                m_IsMusicVolumeHalvedForPause = false;
                PlayMusic(m_MenuMusic, true);
                break;

            case UIManager.ScreenType.HUD:
                m_IsMusicVolumeHalvedForPause = false;
                ApplyMusicVolume();

                if (previousScreen != UIManager.ScreenType.PauseScreen || !m_MusicSource.isPlaying)
                {
                    m_IsPlayingLevelPlaylist = true;
                    PlayNextLevelTrack();
                }
                break;

            case UIManager.ScreenType.PauseScreen:
                if (m_MusicSource.clip != null)
                {
                    m_IsMusicVolumeHalvedForPause = true;
                    ApplyMusicVolume();
                }
                break;
            
            case UIManager.ScreenType.OptionsScreen:
                m_IsMusicVolumeHalvedForPause = false;
                ApplyMusicVolume();
                break;

            case UIManager.ScreenType.DeathScreen:
                m_IsPlayingLevelPlaylist = false;
                m_IsMusicVolumeHalvedForPause = false;
                PlayMusic(m_LoseClip, false);
                break;

            case UIManager.ScreenType.LevelCompleteScreen:
                if (m_IsPlayingLevelPlaylist)
                {
                    m_CurrentLevelMusicIndex++;
                }
                m_IsPlayingLevelPlaylist = false;
                m_IsMusicVolumeHalvedForPause = false;
                PlayMusic(m_WinClip, false);
                break;

            default:
                if (newScreen != UIManager.ScreenType.OptionsScreen) 
                {
                    m_IsPlayingLevelPlaylist = false;
                    m_IsMusicVolumeHalvedForPause = false;
                    ApplyMusicVolume();

                    if (m_MusicSource.isPlaying && m_MusicSource.loop)
                    {
                        m_MusicSource.Stop();
                    }
                }
                break;
        }
    }

    private void PlayNextLevelTrack()
    {
        if (m_MusicSource == null) return;

        if (m_LevelMusicPlaylist == null || m_LevelMusicPlaylist.Count == 0)
        {
            m_MusicSource.Stop();
            m_IsPlayingLevelPlaylist = false;
            return;
        }

        if (m_CurrentLevelMusicIndex >= m_LevelMusicPlaylist.Count || m_CurrentLevelMusicIndex < 0)
        {
            m_CurrentLevelMusicIndex = 0;
        }

        AudioClip clipToPlay = m_LevelMusicPlaylist[m_CurrentLevelMusicIndex];

        if (clipToPlay != null)
        {
            m_MusicSource.Stop();
            m_MusicSource.clip = clipToPlay;
            m_MusicSource.loop = false;
            ApplyMusicVolume();
            m_MusicSource.Play();
        }
        else
        {
            m_CurrentLevelMusicIndex++;
            PlayNextLevelTrack();
        }
    }

    public void PlayMusic(AudioClip clip, bool loop)
    {
        if (m_MusicSource == null) return;

        m_IsPlayingLevelPlaylist = false;

        if (clip != null)
        {
            if (m_MusicSource.clip == clip && m_MusicSource.isPlaying && m_MusicSource.loop == loop)
            {
                ApplyMusicVolume();
                return;
            }

            m_MusicSource.Stop();
            m_MusicSource.clip = clip;
            m_MusicSource.loop = loop;
            ApplyMusicVolume();
            m_MusicSource.Play();
        }
        else
        {
            m_MusicSource.Stop();
        }
    }

    /// <summary>
    /// Manually skips to the next track in the level music playlist.
    /// Only works if the level music playlist is currently active.
    /// </summary>
    public void SkipToNextLevelTrack()
    {
        if (!m_IsPlayingLevelPlaylist)
        {
            Debug.LogWarning("Cannot skip track: Level music playlist is not currently active.");
            return;
        }

        if (m_LevelMusicPlaylist == null || m_LevelMusicPlaylist.Count == 0)
        {
            Debug.LogWarning("Cannot skip track: Level music playlist is empty.");
            return;
        }

        m_CurrentLevelMusicIndex++;
        // PlayNextLevelTrack will handle wrapping the index and playing the track.
        PlayNextLevelTrack();
        Debug.Log("Skipped to the next level music track.");
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