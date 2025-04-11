using System;
using UnityEngine;

[System.Serializable]
class ActionTimer
{
    public bool IsRunning => m_IsRunning;
    public bool m_IsRunning;
    public float ElapsedTime => m_ElapsedTime;

    private Action m_TimerFinishedCallback;
     
    private float m_ElapsedTime = 0.0f;
    private float m_Duration = 0.0f;
    private float m_PreviousDuration = 0.0f;
    private bool m_IsUnscaled = false;
    public ActionTimer(Action onTimerFinished, bool unscaled = false)
    {
        m_IsUnscaled = unscaled;
        SetCallback(onTimerFinished);
    }

    public ActionTimer(bool unscaled = false)
    {
        m_IsUnscaled = unscaled;
    }

    public void SetCallback(Action onTimerFinsihed)
    {
        m_TimerFinishedCallback = onTimerFinsihed;
    }

    public void Start(float duration)
    {
        if (duration < 0.0f)
            throw new ArgumentOutOfRangeException("duration", "Duration must be greater than 0");

        m_ElapsedTime = 0.0f;
        m_Duration = duration;
        m_IsRunning = true;
    }

    public void Cancel()
    {
        if (!m_IsRunning)
            throw new InvalidOperationException("Timer is not running");

        Reset();
    }

    public void Stop()
    {
        Reset();
    }

    public void Restart()
    {
        Reset();
        Start(m_PreviousDuration);
    }

    public void Tick()
    {
        if (m_IsRunning)
        {
            m_ElapsedTime += m_IsUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;

            if (m_ElapsedTime >= m_Duration)
            {
                Reset();

                m_TimerFinishedCallback?.Invoke();
            }
        }
    }

    private void Reset()
    {
        m_ElapsedTime = 0.0f;
        m_IsRunning = false;

        m_PreviousDuration = m_Duration != 0.0f ? m_Duration : m_PreviousDuration;
        m_Duration = 0.0f;
    }
}
