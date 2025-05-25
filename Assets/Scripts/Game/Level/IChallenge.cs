namespace TheGame
{
    public enum ChallengeStatus
    {
        None,
        NotStarted,
        InProgress,
        Complete,
        Failed
    }
    public interface IChallenge
    {
        string ID { get; }
        string Name { get; }

        public ChallengeStatus Status { get; }

        void Initialize(ChallengeSaveData saveData, LevelData levelData);
        void OnLevelStarted();
        void OnUpdate(float detlatTime);
        void OnLevelCompleted();
        void OnLevelExited();
        void OnLevelRestarted();
        ChallengeSaveData GetSaveData();
    }

    [System.Serializable]
    public class ChallengeSaveData
    {
        public string ID;
        public ChallengeStatus Status;
    }
}