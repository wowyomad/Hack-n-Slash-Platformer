using UnityEngine;

public class ThrowableEffectSound : MonoBehaviour, IThrowableEffect
{
    public AudioClip Sound;
    private AudioSource m_AudioSource;

    private void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    public void ApplyEffect(GameObject collidedObject)
    {
        if (Sound != null)
        {
            m_AudioSource.PlayOneShot(Sound);
        }
    }
}
