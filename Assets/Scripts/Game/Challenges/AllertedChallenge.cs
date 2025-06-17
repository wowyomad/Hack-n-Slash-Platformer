using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace TheGame
{
    [CreateAssetMenu(fileName = "AllertedChallenge", menuName = "Game/Challenges/Allerted")]
    public class AllertedChallenge : Challenge
    {
        private class AlertedData
        {
            public Enemy Enemy;
            public float Duration;
        }
        public float AllowedAlertedDuration = 2.0f;
        private List<AlertedData> m_AlertedEnemies = new List<AlertedData>();
        public override void OnLevelLoaded()
        {
            EventBus<EnemyAlertedEvent>.OnEvent += OnEnemyAlerted;
        }
        public override void OnLevelExited()
        {
            EventBus<EnemyAlertedEvent>.OnEvent -= OnEnemyAlerted;
        }

        public override void OnLevelStarted()
        {
            m_AlertedEnemies.Clear();
            Status = ChallengeStatus.InProgress;

            Debug.Log($"Challenge {Name} started. Allowed alerted duration: {AllowedAlertedDuration} seconds.");
        }

        public override void OnUpdate(float deltaTime)
        {
            if (Status == ChallengeStatus.InProgress)
            {
                m_AlertedEnemies.RemoveAll(e => e.Enemy.IsDead);
                m_AlertedEnemies.ForEach(e => e.Duration += deltaTime);

                if (m_AlertedEnemies.Any(e => e.Duration >= AllowedAlertedDuration))
                {
                    Debug.Log($"Challenge {Name} failed due to alerted enemies exceeding allowed {AllowedAlertedDuration}.");
                    Status = ChallengeStatus.Failed;

                    FindAnyObjectByType<UIManager>().DisplayChallengePopup(this, false);
                    FindAnyObjectByType<AudioManager>().PlayChallengeFailedSFX();
                }
            }

        }

        public override void OnLevelCompleted()
        {
            if (Status == ChallengeStatus.InProgress)
            {
                Debug.Log($"Challenge {Name} completed successfully.");
                Status = ChallengeStatus.Complete;
            }
        }



        private void OnEnemyAlerted(EnemyAlertedEvent @event)
        {
            if (@event.Alerted)
            {
                Debug.Log($"Enemy alerted: {@event.EnemyGameObject.name}");
                if (m_AlertedEnemies.SingleOrDefault(e => e.Enemy.gameObject == @event.EnemyGameObject) != null)
                {
                    return;
                }

                m_AlertedEnemies.Add(new AlertedData
                {
                    Enemy = @event.EnemyGameObject.GetComponent<Enemy>(),
                    Duration = 0.0f
                });
            }
            else
            {
                m_AlertedEnemies.RemoveAll(e => e.Enemy.gameObject == @event.EnemyGameObject);
            }

        }
    }

}
