using UnityEngine;

public class TestEventRaiser : MonoBehaviour
{
    public float TimeBetweenEvents = 5.0f;
    public float RandomRange = 2.0f;
    public bool RaiseEvents = true;

    private float m_TimeSinceLastEvent = 0.0f;
    private float m_TimeToNextEvent = 0.0f;
    private int m_Counter = 0;

    private void Start()
    {
        m_TimeToNextEvent = GetRandomTime();
    }

    private void Update()
    {
        if (!RaiseEvents)
        {
            return;
        }

        m_TimeSinceLastEvent += Time.deltaTime;

        if (m_TimeSinceLastEvent >= m_TimeToNextEvent)
        {
            m_TimeSinceLastEvent = 0.0f;
            EventBus<TestEvent>.Raise(new TestEvent() { message = $"Event: {++m_Counter}" });

            m_TimeToNextEvent = GetRandomTime();
        }
    }

    private float GetRandomTime()
    {
        return TimeBetweenEvents + UnityEngine.Random.Range(-RandomRange, RandomRange);
    }
}
