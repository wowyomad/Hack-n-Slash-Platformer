using System.Collections.Generic;
using System.Linq;
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
            EventBus<LevelFinishReachedEvent>.OnEvent += CompleteBecauseLevelFinishReached;
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
            Challenges.ForEach(challenge => challenge.OnLevelExited());
            EventBus<PlayerDiedEvent>.OnEvent -= FailBecausePlayedDied;
            EventBus<LevelFinishReachedEvent>.OnEvent -= CompleteBecauseLevelFinishReached;
        }

        public virtual void OnLevelStarted()
        {
            Debug.Log($"Level {Name} started.");

            LevelStatus = Status.InProgress;
            Challenges.ForEach(challenge =>
            {
                if (challenge.Status != ChallengeStatus.Complete)
                {
                    challenge.OnLevelStarted();
                }
            });
        }

        public virtual void OnUpdate(float deltaTime)
        {
            if (LevelStatus == Status.InProgress)
            {
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
            Challenges.ForEach(challenge =>
                {
                    if (challenge.Status != ChallengeStatus.Complete)
                    {
                        challenge.OnLevelRestarted();
                    }
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
        }

        protected void FailBecausePlayedDied(PlayerDiedEvent e)
        {
            OnLevelFailed();
        }

        protected void CompleteBecauseLevelFinishReached(LevelFinishReachedEvent e)
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
