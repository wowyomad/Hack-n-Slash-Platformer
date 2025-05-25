using UnityEngine;

namespace TheGame
{
    [CreateAssetMenu(fileName = "PlaceHolderChallenge", menuName = "Challenges/PlaceHolder Challenge")]
    public class PlaceHolderChallenge : Challenge
    {
        [SerializeField] private float m_TimeToFinish = 20.0f;

        float m_ElapsedTime = 0.0f;
        public override void OnLevelStarted()
        {
            if (ChallengeStatus == ChallengeStatus.Complete)
            {
                return;
            }

            m_ElapsedTime = 0.0f;
            ChallengeStatus = ChallengeStatus.InProgress;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (ChallengeStatus == ChallengeStatus.InProgress)
            {
                m_ElapsedTime += deltaTime;

                if (m_ElapsedTime >= m_TimeToFinish)
                {
                    ChallengeStatus = ChallengeStatus.Failed;
                }
            }
        }

        public override void OnLevelCompleted()
        {
            if (ChallengeStatus == ChallengeStatus.InProgress)
            {
                ChallengeStatus = ChallengeStatus.Complete;
                Debug.Log($"Challenge {Name} completed successfully.");
            }
            else
            {
                Debug.LogWarning($"Challenge {Name} was not completed before the level ended.");
            }
        }

        public override void OnLevelExited()
        {
            if (ChallengeStatus == ChallengeStatus.InProgress)
            {
                ChallengeStatus = ChallengeStatus.NotStarted;
            }
        }
        public override void Initialize(ChallengeSaveData saveData, LevelData levelData)
        {
            base.Initialize(saveData, levelData);
        }
    }
}