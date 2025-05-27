using TheGame;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
    public CameraShakeSettings Settings;

    private Camera m_Camera;

    private Vector3 m_OriginalPosition;
    private float m_CurrentShakeStrength = 0f;
    private float m_StrengthVelocity = 0f;
    private bool m_IsShaking = false;

    private void Awake()
    {
        m_Camera = GetComponent<Camera>();
        m_OriginalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        EventBus<EnemyGotHitEvent>.OnEvent += OnEnemyHitEvent;
        EventBus<PlayerGotHitEvent>.OnEvent += OnPlayerHitEvent;
        EventBus<EnemyGotParriedEvent>.OnEvent += OnEnemyParriedEvent;
        EventBus<DoorOpenedWithHitEvent>.OnEvent += OnOpenedByHitEvent;
    }

    private void OnDisable()
    {
        EventBus<EnemyGotHitEvent>.OnEvent -= OnEnemyHitEvent;
        EventBus<PlayerGotHitEvent>.OnEvent -= OnPlayerHitEvent;
        EventBus<EnemyGotParriedEvent>.OnEvent -= OnEnemyParriedEvent;
        EventBus<DoorOpenedWithHitEvent>.OnEvent -= OnOpenedByHitEvent;
    }

    private void Update()
    {
        if (Settings == null) return;

        if (m_IsShaking)
        {
            float randomX = (Random.value - 0.5f) * 2f * Settings.RandomnessMultiplier;
            float randomY = (Random.value - 0.5f) * 2f * Settings.RandomnessMultiplier;

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
            m_CurrentShakeStrength = 0f;
            m_StrengthVelocity = 0f;
        }
    }

    private void StopShake()
    {
        m_IsShaking = false;
        transform.localPosition = m_OriginalPosition;
        m_CurrentShakeStrength = 0f;
        m_StrengthVelocity = 0f;
    }

    private void OnOpenedByHitEvent(DoorOpenedWithHitEvent e)
    {
        TriggerShake(e.DoorPosition, Settings.DoorOpenedShakeStrength);
    }

    private void OnEnemyHitEvent(EnemyGotHitEvent e)
    {
        TriggerShake(e.EnemyPosition, Settings.EnemyHitShakeStrength);
    }

    private void OnEnemyParriedEvent(EnemyGotParriedEvent e)
    {
        TriggerShake(e.EnemyPosition, Settings.EnemyParriedShakeStrength);
    }

    private void OnPlayerHitEvent(PlayerGotHitEvent e)
    {
        TriggerShake(e.PlayerPosition, Settings.PlayerHitShakeStrength);
    }

    public void TriggerShake()
    {
        if (Settings == null)
        {
            Debug.LogWarning("Shake Settings not assigned to " + gameObject.name);
            return;
        }
        ApplyShakeStrength(Settings.ShakeStrength);
    }

    private void TriggerShake(Vector3 sourcePosition, float strength = 0.0f)
    {
        if (strength <= 0.0f)
        {
            strength = Settings.ShakeStrength;
        }

        if (Settings.IsScaledOnDistance)
        {
            float distance = Vector3.Distance(sourcePosition, m_Camera.transform.position); // Use Vector3.Distance for 3D
            float distanceFactor = Mathf.InverseLerp(0f, Settings.MaxDistance, distance);
            float strengthMultiplier = Mathf.Lerp(1f, Settings.MaxDistanceMultiplier, distanceFactor);
            strength *= strengthMultiplier;
        }

        ApplyShakeStrength(strength);
    }

    public void SetShake(float newStrength)
    {
        ApplyShakeStrength(newStrength);
    }


    private void ApplyShakeStrength(float strengthToAdd)
    {
        if (Settings == null) return;

        if (m_IsShaking)
        {
            m_CurrentShakeStrength += strengthToAdd * Settings.OngoingShakeMultiplier;
        }
        else
        {
            m_CurrentShakeStrength = Mathf.Max(m_CurrentShakeStrength, strengthToAdd);
            m_IsShaking = true;
            m_StrengthVelocity = 0f;
        }
    }
}