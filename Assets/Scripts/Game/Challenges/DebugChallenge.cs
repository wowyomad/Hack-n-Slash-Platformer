using TheGame;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugChallenge", menuName = "Game/Challenges/Debug Challenge")]
public class DebugChallenge : Challenge
{
    private InputReader m_Input;
    public override void OnLevelLoaded()
    {
        Debug.Log("Debug Challenge Level Loaded");
    }
    public override void OnLevelCompleted()
    {
        Debug.Log("Debug Challenge Level Completed");
    }
    public override void OnLevelFailed()
    {
        Debug.Log("Debug Challenge Level Failed");
    }
    public override void OnLevelExited()
    {
        Debug.Log("Debug Challenge Level Exited");

        m_Input.DebugGood -= CompleteChallenge;
        m_Input.DebugBad -= FailChallenge;
    }
    public override void OnLevelRestarted()
    {
        Debug.Log("Debug Challenge Level Restarted");
        Status = ChallengeStatus.InProgress;
    }

    public override void OnLevelStarted()
    {
        Debug.Log("Debug Challenge Level Started");
        Status = ChallengeStatus.InProgress;
    }

    public override void Initialize(ChallengeSaveData data)
    {
        base.Initialize(data);
        m_Input = InputReader.Load();

        m_Input.DebugGood += CompleteChallenge;
        m_Input.DebugBad += FailChallenge;
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