using UnityEngine;

public class ThrowableEffectSound : MonoBehaviour, IThrowableImpactEffect
{
    public AudioClip Sound;
    private AudioSource m_AudioSource;

    private void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    public void ApplyImpactEffect(GameObject collidedObject, Vector2 position, Vector2 normal)
    {
        if (Sound != null)
        {
            m_AudioSource.PlayOneShot(Sound);
        }
    }
}
