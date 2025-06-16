using TheGame;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugChallenge", menuName = "Game/Challenges/Debug")]
public class LeverChallenge : Challenge
{
    private bool m_JustCompleted = false;
    [SerializeField] private int m_HitsRequired = 7;
    private int m_HitsCount = 0;
    public override void OnLevelLoaded()
    {
        EventBus<LeverHitEvent>.OnEvent += OnLeverHit;
    }

    public override void OnLevelExited()
    {
        EventBus<LeverHitEvent>.OnEvent -= OnLeverHit;
    }

    public override void OnLevelStarted()
    {
        Status = ChallengeStatus.InProgress;

        m_JustCompleted = false;
        m_HitsCount = 0;

        Debug.Log($"Challenge {Name} started.");
    }

    public override void OnLevelCompleted()
    {
        if (m_JustCompleted && Status != ChallengeStatus.Complete)
        {
            Status = ChallengeStatus.Complete;
            Debug.Log($"Challenge {Name} completed successfully after {m_HitsCount} hits.");
        }
    }

    private void OnLeverHit(LeverHitEvent @event)
    {
        if (m_JustCompleted || Status == ChallengeStatus.Complete)
        {
            return;
        }

        m_HitsCount++;

        if (m_HitsCount >= m_HitsRequired)
        {
            Debug.Log($"Challenge {Name} completed after {m_HitsCount} hits.");
            m_JustCompleted = true;
            FindAnyObjectByType<UIManager>().DisplayChallengePopup(this, true);
        }
        else
        {
            Debug.Log($"Hits count: {m_HitsCount}/{m_HitsRequired}");
        }
    }
}