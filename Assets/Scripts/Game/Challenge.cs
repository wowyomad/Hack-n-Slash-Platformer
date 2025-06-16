using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        public Ability RewardAbility;
        public ChallengeStatus Status = ChallengeStatus.Incomplete;

        public virtual void Initialize(ChallengeSaveData data)
        {
            if (data != null)
            {
                Status = data.Completed ? ChallengeStatus.Complete : ChallengeStatus.Incomplete;
            }
            else
            {
                Status = ChallengeStatus.Incomplete;
            }
        }
        public virtual void OnLevelLoaded() { }
        public virtual void OnLevelStarted() { }
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnLevelCompleted() { }
        public virtual void OnLevelFailed() { }
        public virtual void OnLevelExited() { }
        public virtual void OnLevelRestarted() { }

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