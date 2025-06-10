using TheGame;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugChallenge", menuName = "Game/Challenges/Debug Challenge")]
public class DebugChallenge : Challenge
{
    public InputReader m_Input;
    public override void OnLevelLoaded()
    {
        Debug.Log("Debug Challenge Level Loaded");
    }
    public override void OnLevelCompleted()
    {
        Debug.Log("Debug Challenge Level Completed");

        m_Input.DebugGood -= CompleteChallenge;
        m_Input.DebugBad -= FailChallenge;
    }
    public override void OnLevelFailed()
    {
        Debug.Log("Debug Challenge Level Failed");

        m_Input.DebugGood -= CompleteChallenge;
        m_Input.DebugBad -= FailChallenge;
    }
    public override void OnLevelExited()
    {
        Debug.Log("Debug Challenge Level Exited");

    }
    public override void OnLevelRestarted()
    {
        Debug.Log("Debug Challenge Level Restarted");
        Status = ChallengeStatus.InProgress;


        m_Input.DebugGood += CompleteChallenge;
        m_Input.DebugBad += FailChallenge;
    }

    public override void OnLevelStarted()
    {
        Debug.Log("Debug Challenge Level Started");
        Status = ChallengeStatus.InProgress;

        m_Input.DebugGood += CompleteChallenge;
        m_Input.DebugBad += FailChallenge;
    }

    public override void Initialize(ChallengeSaveData data)
    {
        base.Initialize(data);
        m_Input = InputReader.Load();

    }
    private void CompleteChallenge()
    {
        Status = ChallengeStatus.Complete;
        Debug.Log("Debug Challenge Completed");

    }

    private void FailChallenge()
    {
        Status = ChallengeStatus.Failed;
        Debug.Log("Debug Challenge Failed");
    }
}