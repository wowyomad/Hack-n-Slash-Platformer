using UnityEngine;

namespace TheGame
{
    public abstract class Challenge : ScriptableObject, IChallenge
    {
        public string ID => ChallengeID;
        public string Name => ChallengeName;
        public ChallengeStatus Status => ChallengeStatus;


        [SerializeField] protected string ChallengeID;
        [SerializeField] protected string ChallengeName;
        protected ChallengeStatus ChallengeStatus;
        protected LevelData OwningLevel;
        public virtual void Initialize(ChallengeSaveData saveData, LevelData levelData)
        {
            OwningLevel = levelData;
            if (saveData != null)
            {
                ChallengeStatus = saveData.Status;
            }
            else
            {
                ChallengeStatus = ChallengeStatus.NotStarted;
            }
        }
        public virtual void OnLevelStarted()
        { }
        public virtual void OnUpdate(float deltaTime)
        { }
        public virtual void OnLevelCompleted()
        { }
        public virtual void OnLevelExited()
        { }
        public virtual void OnLevelRestarted() => OnLevelStarted();
        public virtual ChallengeSaveData GetSaveData()
        {
            return new ChallengeSaveData
            {
                ID = ChallengeID,
                Status = ChallengeStatus
            };
        }

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(ChallengeID))
            {
                ChallengeID = System.Guid.NewGuid().ToString();
            }
        }
    }
}