using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheGame
{
    public class LevelManager : PersistentSingleton<LevelManager>
    {
        public bool IsLevelActive => CurrentLevel != null;
        public string SaveName = "SaveData.json";
        public LevelPresetsData LevelPresets;
        public List<Level> RuntimeLevels = new List<Level>();
        public GameSaveData SaveData = new GameSaveData();
        public Level CurrentLevel;
        private string m_SaveFilePath;
        private GameManager m_GameManager;

        private bool m_Running = false;

        protected override void Awake()
        {
            if (!HasInstance)
            {
                m_GameManager = GetComponent<GameManager>();
                m_SaveFilePath = System.IO.Path.Combine(Application.persistentDataPath, SaveName);
            }
            base.Awake();
        }

        protected void Start()
        {
            InitializeLoadSaveData();
        }

        protected void Update()
        {
            if (CurrentLevel == null)
            {
                return;
            }

            if (m_Running)
            {
                CurrentLevel.OnUpdate(Time.deltaTime);
                
                if (CurrentLevel.LevelStatus == Level.Status.Complete)
                {
                    m_Running = false;

                    if (!CurrentLevel.Completed)
                    {
                        CurrentLevel.Completed = true;

                        CurrentLevel.NextLevels.ForEach(nextLevel =>
                        {
                            if (nextLevel != null && !nextLevel.Opened)
                            {
                                nextLevel.Opened = true;
                            }
                        });
                    }

                }
                else if (CurrentLevel.LevelStatus == Level.Status.Failed)
                {
                    m_Running = false;
                    Save();
                }
            }


        }

        private void OnEnable()
        {
            m_GameManager.LoadCompleted += OnSceneLoadCompleted;
        }

        private void OnDisable()
        {
            m_GameManager.LoadCompleted -= OnSceneLoadCompleted;
        }

        public void InitializeLoadSaveData()
        {
            if (LevelPresets == null)
            {
                Debug.LogError("LevelPresets is not assigned in LevelManager. Please assign it in the inspector.");
                return;
            }

            if (System.IO.File.Exists(m_SaveFilePath))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(m_SaveFilePath);
                    SaveData = JsonUtility.FromJson<GameSaveData>(json);

                    SaveData.Levels.RemoveAll(level => level.ID == null || LevelPresets.Levels.All(preset => preset.ID != level.ID));
                    if (SaveData.Levels.All(level => level.ID != SaveData.LastUnfinishedLevelID))
                    {
                        SaveData.LastUnfinishedLevelID = string.Empty;
                    }

                    if (string.IsNullOrEmpty(SaveData.LastUnfinishedLevelID))
                    {
                        SaveData.LastUnfinishedLevelID = SaveData.Levels != null && SaveData.Levels.Count > 0 ? SaveData.Levels[0].ID : LevelPresets.Levels[0].ID;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load save data from {m_SaveFilePath}: {e.Message}");
                }
            }
            RuntimeLevels = LevelPresets.Levels.Select(preset => Instantiate(preset)).ToList();

            RuntimeLevels.ForEach(level =>
            {
                level.Challenges = level.Challenges.Select(challenge => Instantiate(challenge)).ToList();
                var nextLevels = level.NextLevels.Select(nextLevel => RuntimeLevels.SingleOrDefault(l => l.ID == nextLevel.ID)).Where(resolved => resolved != null).ToList();
                if (nextLevels.Count != level.NextLevels.Count)
                {
                    Debug.LogWarning(
                        $"Some next levels in \"{level.Name}\" could not be resolved.\n" +
                        $"Preset contains: [{string.Join(", ", level.NextLevels.Select(l => $"\"{l.Name}\""))}]\n" +
                        $"Resolved contains: [{string.Join(", ", nextLevels.Select(l => $"\"{l.Name}\""))}]"
                    );
                }
                level.NextLevels = nextLevels;
            });

            // load saves
            RuntimeLevels.ForEach(level =>
            {
                LevelSaveData levelSaveData = SaveData.Levels.FirstOrDefault(s => s.ID == level.ID);
                if (levelSaveData != null)
                {
                    level.Opened = levelSaveData.Opened;
                    level.Completed = levelSaveData.Completed;

                    if (levelSaveData.Challenges != null)
                    {
                        level.Challenges.ForEach(challenge =>
                        {
                            ChallengeSaveData challengeSaveData = levelSaveData.Challenges.FirstOrDefault(c => c.ID == challenge.ID);
                            if (challengeSaveData != null)
                            {
                                challenge.Initialize(challengeSaveData);
                            }
                            else
                            {
                                Debug.LogWarning($"Challenge \"{challenge.Name}\" in level \"{level.Name}\" has no save data.");
                            }
                        });
                    }
                }
                else
                {
                    Debug.LogWarning($"Level \"{level.Name}\" has no save data.");
                }
            });
        }

        public void LoadLevelByID(string levelID)
        {
            var level = RuntimeLevels.FirstOrDefault(l => l.ID == levelID);
            if (level == null)
            {
                Debug.LogError($"Level with ID {levelID} not found.");
                return;
            }

            LoadLevel(level);
        }

        public void LoadLevel(Level level)
        {
            if (level == null || string.IsNullOrEmpty(level.SceneName))
            {
                Debug.LogError("Level is null or scene name is missing.");
                return;
            }
            if (CurrentLevel != null && CurrentLevel.ID == level.ID)
            {
                Debug.LogWarning($"Level {level.Name} is already loaded.");
                return;
            }
            CurrentLevel?.OnLevelExited();
            CurrentLevel = level;
            m_GameManager.LoadGameLevel(level.SceneName);
        }

        public void LoadLastUnfinishedLevel()
        {
            LoadLevelByID(SaveData.LastUnfinishedLevelID);
        }

        public void RestartCurrentLevel()
        {
            if (CurrentLevel == null)
            {
                Debug.LogError("Current level is not set.");
                return;
            }

            m_GameManager.LoadGameLevel(CurrentLevel.SceneName);
            CurrentLevel.OnLevelRestarted();
            m_GameManager.ResumeGame();
        }

        public void Save()
        {
            if (RuntimeLevels == null || RuntimeLevels.Count == 0)
            {
                Debug.LogWarning("No data to save.)");
                return;
            }

            List<LevelSaveData> levelSaves = RuntimeLevels.Select(level => new LevelSaveData
            {
                ID = level.ID,
                Opened = level.Opened,
                Completed = level.Completed,
                Challenges = level.Challenges.Select(challenge => challenge.GetSaveData()).ToList()
            }).ToList();

            SaveData.Levels = levelSaves;
            SaveData.LastUnfinishedLevelID = CurrentLevel?.ID ?? string.Empty;

            string json = JsonUtility.ToJson(SaveData);

            System.IO.File.WriteAllText(m_SaveFilePath, json);
        }

        private void OnSceneLoadCompleted()
        {
            if (CurrentLevel != null && SceneManager.GetActiveScene().name == CurrentLevel.SceneName)
            {
                CurrentLevel.OnLevelLoaded();
                CurrentLevel.OnLevelStarted();

                Save();

                m_GameManager.ResumeGame();
                m_Running = true;
            }
            else
            {
                Debug.LogWarning("No current level set when scene load completed.");
            }
        }
    }
}