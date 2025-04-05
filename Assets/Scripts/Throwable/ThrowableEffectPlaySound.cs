using UnityEngine;

public class ThrowableEffectSound : MonoBehaviour, IThrowableImpactEffect
{
    public AudioClip Sound;
    private AudioSource m_AudioSource;

    private void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    public void ApplyImpactEffect(GameObject victim, Vector2 point, Vector2 normal)
    {
        if (Sound != null)
        {
            m_AudioSource.PlayOneShot(Sound);
        }
    }
}
