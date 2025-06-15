using System.Collections.Generic;
using TheGame;
using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(IHittable))]
public class HitAudio : MonoBehaviour
{
    [SerializeField] private List<AudioClip> m_HitSounds;
    [SerializeField] private bool m_Random = true;
    [SerializeField] private AudioSource m_AudioSource;

    private IHittable m_Hittable;
    private int m_LastPlayedIndex = -1;

    private void Awake()
    {
        m_Hittable = GetComponent<IHittable>();
        m_AudioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (m_Hittable != null)
        {
            m_Hittable.OnHit += PlayHitSound;
        }
        else
        {
            Debug.LogError("IHittable component not found on HitAudio GameObject.", this);
        }
    }

    private void OnDisable()
    {
        if (m_Hittable != null)
        {
            m_Hittable.OnHit -= PlayHitSound;
        }
    }

    private void PlayHitSound()
    {
        if (m_AudioSource != null && m_HitSounds.Count > 0)
        {
            AudioClip clip;
            if (m_Random)
            {
                clip = GetRandomHitSound();
            }
            else
            {
                m_LastPlayedIndex = (m_LastPlayedIndex + 1) % m_HitSounds.Count;
                clip = m_HitSounds[m_LastPlayedIndex];
            }
            m_AudioSource.PlayOneShot(clip);
        }
    }

    private AudioClip GetRandomHitSound()
    {
        if (m_HitSounds.Count > 0)
        {
            int randomIndex = Random.Range(0, m_HitSounds.Count);
            return m_HitSounds[randomIndex];
        }
        return null;
    }

    [ContextMenu("Test Hit Sound")]
    private void TestHitSound()
    {
        PlayHitSound();
    }
}
