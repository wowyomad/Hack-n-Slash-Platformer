using TheGame;
using UnityEngine;

public class RageMeter : MonoBehaviour
{
    public bool InUse = true;
    [SerializeField] private float m_MaxValue = 1.0f;
    [SerializeField] private float m_ScorePerEnemyHit = 0.6f;
    [SerializeField] private float m_ScoreDecreaseRate = 0.2f;
    [SerializeField] private float m_ScoreDecreaseDelay = 2.0f;
    public bool IsFull => m_Value >= m_MaxValue;
    [SerializeField] private float m_Value;
    private float m_ScoreDecreaseTimer;

    private void OnEnable()
    {
        EventBus<EnemyGotHitEvent>.OnEvent += OnEnemyHit;
    }
    private void OnDisable()
    {
        EventBus<EnemyGotHitEvent>.OnEvent -= OnEnemyHit;
    }


    private void Update()
    {
        if (m_Value > 0.0f)
        {
            m_ScoreDecreaseTimer += Time.deltaTime;
            if (m_ScoreDecreaseTimer >= m_ScoreDecreaseDelay)
            {
                m_Value -= m_ScoreDecreaseRate * Time.deltaTime;
                if (m_Value < 0.0f)
                {
                    m_Value = 0.0f;
                }
            }
        }
        else
        {
            m_ScoreDecreaseTimer = 0.0f;
        }
    }

    private void OnEnemyHit(EnemyGotHitEvent obj)
    {
        m_ScoreDecreaseTimer = 0.0f;
        Add(m_ScorePerEnemyHit);
    }

    public void Add(float value)
    {
        m_Value += value;
        if (m_Value > 1.0f)
        {
            m_Value = 1.0f;
        }
    }
}