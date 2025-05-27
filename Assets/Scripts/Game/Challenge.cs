using UnityEngine;

namespace TheGame
{
    public enum ChallengeStatus
    {
        None,
        Incomplete,
        InProgress,
        Complete,
        Failed
    }

    public abstract class Challenge : ScriptableObject
    {
        [Header("Template")]
        public string ID = System.Guid.NewGuid().ToString();
        public string Name = "New Challenge";
        public string Description = "Challenge Description";
        public ChallengeStatus Status;

        public virtual void Initialize(ChallengeSaveData data)
        {
            if (data != null)
            {
                Status = data.Completed ? ChallengeStatus.Complete : ChallengeStatus.Incomplete;
            }
            else
            {
                throw new System.ArgumentNullException(nameof(data), "ChallengeSaveData cannot be null");
            }
        }
        public virtual void OnLevelLoaded() { }
        public virtual void OnLevelStarted() { }
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnLevelCompleted() { }
        public virtual void OnLevelFailed() { }
        public virtual void OnLevelExited() { }
        public virtual void OnLevelRestarted() => OnLevelStarted();

        public virtual ChallengeSaveData GetSaveData()
        {
            return new ChallengeSaveData
            {
                ID = ID,
                Completed = Status == ChallengeStatus.Complete
            };
        }
    }
}