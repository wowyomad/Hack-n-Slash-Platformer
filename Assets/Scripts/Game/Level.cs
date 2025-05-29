using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

namespace TheGame
{
    public class Level : ScriptableObject
    {
        public string ID = System.Guid.NewGuid().ToString();
        public string Name;
        public string Description;
        public bool Opened;
        public bool Completed;
        public string SceneName;
        public SceneReference SceneReference;
        public List<Challenge> Challenges = new List<Challenge>();
        public List<Level> NextLevels = new List<Level>();
        public Status LevelStatus = Status.None;
        public float TimeToComplete = 60.0f;
        public float TimeElsaped => Time.time - m_TimeStarted;
        public float TimeRemaining => Mathf.Max(0.0f, TimeToComplete - TimeElsaped);


        [NonSerialized]
        private float m_TimeStarted;


        public enum Status
        {
            None,
            Complete,
            Failed,
            InProgress
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            SceneReference.OnValueChanged += OnValidate;
        }
        private void OnDisable()
        {
            SceneReference.OnValueChanged -= OnValidate;
        }
#endif

        public virtual void OnLevelLoaded()
        {
            Debug.Log($"Level {Name} loaded.");
            Challenges.ForEach(challenge => challenge.OnLevelLoaded());
            EventBus<PlayerDiedEvent>.OnEvent += FailBecausePlayedDied;
            EventBus<LevelFinishTriggeredEvent>.OnEvent += CompleteBecauseLevelFinished;
            EventBus<TriggerRestartEvent>.OnEvent += RestartLevel;
        }

        public virtual void OnLevelExited()
        {
            if (LevelStatus == Status.InProgress)
            {
                Debug.Log("Level exited before completion.");
                OnLevelFailed();
            }
            else
            {
                Debug.Log($"Level {Name} exited with status: {LevelStatus}");
            }
            if (LevelStatus == Status.Complete)
            {

            }
            Challenges.ForEach(challenge => challenge.OnLevelExited());
            EventBus<PlayerDiedEvent>.OnEvent -= FailBecausePlayedDied;
            EventBus<LevelFinishTriggeredEvent>.OnEvent -= CompleteBecauseLevelFinished;
            EventBus<TriggerRestartEvent>.OnEvent -= RestartLevel;

            EventBus<LevelExitedEvent>.Raise(new LevelExitedEvent
            {
                Level = this,
            });
        }

        public virtual void OnLevelStarted()
        {
            Debug.Log($"Level {Name} started.");

            LevelStatus = Status.InProgress;
            m_TimeStarted = Time.time;
            Challenges.ForEach(challenge =>
            {
                if (challenge.Status != ChallengeStatus.Complete)
                {
                    challenge.OnLevelStarted();
                }
            });

            EventBus<LevelStartedEvent>.Raise(new LevelStartedEvent
            {
                Level = this,
            });
        }

        public virtual void OnUpdate(float deltaTime)
        {
            if (LevelStatus == Status.InProgress)
            {
                if (TimeRemaining <= 0.0f)
                {
                    FailBecauseTimeExpired();
                }

                Challenges.ForEach(challenge =>
                {
                    if (challenge.Status != ChallengeStatus.Complete)
                    {
                        challenge.OnUpdate(deltaTime);
                    }
                });
            }
        }

        public virtual void OnLevelRestarted()
        {
            Debug.Log($"Level {Name} restarted.");
            LevelStatus = Status.InProgress;
            m_TimeStarted = Time.time;
            Challenges.ForEach(challenge =>
                {
                    if (challenge.Status != ChallengeStatus.Complete)
                    {
                        challenge.OnLevelRestarted();
                    }
                });

            EventBus<LevelRestartedEvent>.Raise(new LevelRestartedEvent
            {
                Level = this,
            });
        }

        public virtual void OnLevelCompleted()
        {
            Debug.Log($"Level {Name} completed.");
            LevelStatus = Status.Complete;
            Challenges.ForEach(challenge =>
            {
                if (challenge.Status != ChallengeStatus.Complete)
                {
                    challenge.OnLevelCompleted();
                }
            });

            EventBus<LevelCompletedEvent>.Raise(new LevelCompletedEvent
            {
                Level = this,
            });
        }

        public virtual void OnLevelFailed()
        {
            Debug.Log($"Level {Name} failed.");

            LevelStatus = Status.Failed;
            Challenges.ForEach(challenge =>
            {
                if (challenge.Status != ChallengeStatus.Complete)
                {
                    challenge.OnLevelFailed();
                }
            });

            EventBus<LevelFailedEvent>.Raise(new LevelFailedEvent
            {
                Level = this,
            });
        }

        public void FailBecauseTimeExpired()
        {
            Debug.Log($"Level {Name} failed due to time expiration.");
            OnLevelFailed();

            EventBus<LevelTimeExpiredEvent>.Raise(new LevelTimeExpiredEvent
            {
                Level = this,
            });
        }

        protected void RestartLevel(TriggerRestartEvent e)
        {
            OnLevelRestarted();
        }

        protected void FailBecausePlayedDied(PlayerDiedEvent e)
        {
            OnLevelFailed();
        }

        protected void CompleteBecauseLevelFinished(LevelFinishTriggeredEvent e)
        {
            OnLevelCompleted();
        }



        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ID))
            {
                ID = System.Guid.NewGuid().ToString();
            }
            if (SceneReference != null)
            {
                //get the name from the path
                SceneName = System.IO.Path.GetFileNameWithoutExtension(SceneReference.Path);
            }
        }
    }
}
