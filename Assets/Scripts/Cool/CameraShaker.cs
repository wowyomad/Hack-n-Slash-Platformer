using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public CameraShakerSettings Settings;

    private Vector3 m_OriginalPosition;
    private float m_CurrentShakeStrength = 0f;
    private float m_StrengthVelocity = 0f;
    private bool m_IsShaking = false;

    private void Awake()
    {
        m_OriginalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        EventBus<EnemyHitEvent>.OnEvent += OnEnemyHitEvent;
        EventBus<PlayerHitEvent>.OnEvent += OnPlayerHitEvent;
    }

    private void OnDisable()
    {
        EventBus<EnemyHitEvent>.OnEvent -= OnEnemyHitEvent;
        EventBus<PlayerHitEvent>.OnEvent -= OnPlayerHitEvent;
    }

    private void Update()
    {
        if (m_IsShaking)
        {
            float randomX = (Random.value - 0.5f) * Settings.RandomnessMultiplier;
            float randomY = (Random.value - 0.5f) * Settings.RandomnessMultiplier;

            transform.localPosition = m_OriginalPosition + new Vector3(randomX, randomY, 0f) * m_CurrentShakeStrength;

            m_CurrentShakeStrength = Mathf.SmoothDamp(
                m_CurrentShakeStrength,
                0f,
                ref m_StrengthVelocity,
                Settings.SmoothTime
            );

            if (m_CurrentShakeStrength <= Settings.ShakeBias)
            {
                StopShake();
            }
        }
        else if (transform.localPosition != m_OriginalPosition)
        {
            transform.localPosition = m_OriginalPosition;
        }
    }

    private void StopShake()
    {
        m_IsShaking = false;
        transform.localPosition = m_OriginalPosition;
        m_CurrentShakeStrength = 0f;
    }

    private void OnEnemyHitEvent(EnemyHitEvent e)
    {
        TriggerShake(e.EnemyPosition);
    }

    private void OnPlayerHitEvent(PlayerHitEvent e)
    {
        TriggerShake(e.PlayerPosition);
    }

    public void TriggerShake()
    {
        if (Settings != null)
        {
            if (m_IsShaking)
            {
                m_CurrentShakeStrength += Settings.ShakeStrength * Settings.OngoingShakeMultiplier;
            }
            else
            {
                m_CurrentShakeStrength = Mathf.Max(m_CurrentShakeStrength, Settings.ShakeStrength);
                m_IsShaking = true;
            }
        }
        else
        {
            Debug.LogWarning("Shake Settings not assigned to " + gameObject.name);
        }
    }

    private void TriggerShake(Vector3 position)
    {
        if (Settings != null)
        {
            float strength = Settings.ShakeStrength;
            if (Settings.UseAffectDistance)
            {
                if (Camera.main != null)
                {
                    float distance = Vector3.Distance(position, Camera.main.transform.position);
                    float value = Mathf.Clamp01((Settings.MaxAffectDistance - distance) / Settings.MaxAffectDistance);
                    strength *= value;
                }
                else
                {
                    Debug.LogWarning("No Main Camera found. Cannot calculate distance-based shake.");
                }
            }

            if (m_IsShaking)
            {
                m_CurrentShakeStrength += strength * Settings.OngoingShakeMultiplier;
            }
            else
            {
                m_CurrentShakeStrength = Mathf.Max(m_CurrentShakeStrength, strength);
                m_IsShaking = true;
            }
        }
        else
        {
            Debug.LogWarning("Shake Settings not assigned to " + gameObject.name);
        }
    }

    public void SetShake(float newStrength)
    {
        if (Settings != null)
        {
            if (m_IsShaking)
            {
                m_CurrentShakeStrength += newStrength * Settings.OngoingShakeMultiplier;
            }
            else
            {
                m_CurrentShakeStrength = Mathf.Max(m_CurrentShakeStrength, newStrength);
                m_IsShaking = true;
            }
        }
        else
        {
            Debug.LogWarning("Shake Settings not assigned to " + gameObject.name);
        }
    }
}