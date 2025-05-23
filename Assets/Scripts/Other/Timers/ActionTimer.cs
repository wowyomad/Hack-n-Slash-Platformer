using System;
using UnityEngine;

[System.Serializable]
class ActionTimer
{
    public bool IsRunning => m_Running;
    public bool IsFinished => m_Finished;
    public float ElapsedTime => m_ElapsedTime;

    private Action m_TimerFinishedCallback;
    private Action m_TimerStartdCallback;
    private bool m_Reset;
    private float m_ElapsedTime = 0.0f;
    private float m_Duration = 0.0f;
    private float m_PreviousDuration = 0.0f;
    private bool m_Unscaled = false;
    private bool m_Repeating = false;
    private bool m_Finished = true;
    private bool m_Running = false;


    public ActionTimer(float duration, bool repeating = false, bool unscaled = false)
    {
        m_Unscaled = unscaled;
        m_Repeating = repeating;
        SetDuration(duration);
    }

    public ActionTimer(bool repeating = false, bool unscaled = false)
    {
        m_Unscaled = unscaled;
        m_Repeating = repeating;
    }

    public void SetFinishedCallback(Action onTimerFinsihed)
    {
        m_TimerFinishedCallback = onTimerFinsihed;
    }

    public void SetStartedCallback(Action onTimerStarted)
    {
        m_TimerStartdCallback = onTimerStarted;
    }

    public void SetDuration(float duration)
    {
        if (m_Running)
            throw new InvalidOperationException("Cannot set duration while timer is running");

        m_Duration = m_PreviousDuration = duration;
    }

    public void Start(float duration)
    {
        if (m_Running)
            throw new InvalidOperationException("Timer is already running");

        if (duration < 0.0f)
            throw new ArgumentOutOfRangeException("duration", "Duration must be greater than 0");

        m_ElapsedTime = 0.0f;
        m_Duration = duration;
        m_Running = true;
        m_Reset = false;
        m_Finished = false;

        m_TimerStartdCallback?.Invoke();
    }

    public void Cancel()
    {
        if (!m_Running)
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
        if (m_Running)
        {
            m_ElapsedTime += m_Unscaled ? Time.unscaledDeltaTime : Time.deltaTime;

            if (m_ElapsedTime >= m_Duration)
            {
                m_TimerFinishedCallback?.Invoke();
                m_Finished = true;

                if (m_Repeating)
                {
                    Restart();
                }
                else
                {
                    Reset();
                }
            }
        }
    }

    private void Reset()
    {
        if (m_Reset) return;

        m_Reset = true;

        m_ElapsedTime = 0.0f;
        m_Running = false;

        m_PreviousDuration = m_Duration != 0.0f ? m_Duration : m_PreviousDuration;
        m_Duration = 0.0f;
    }
}
