using UnityEngine;

public class ThrowableEffectSound : MonoBehaviour, IThrowableEffect
{
    public AudioClip Sound;
    private AudioSource m_AudioSource;

    private void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    public void ApplyEffect(GameObject collidedObject, Vector2 position)
    {
        if (Sound != null)
        {
            m_AudioSource.PlayOneShot(Sound);
        }
    }
}
