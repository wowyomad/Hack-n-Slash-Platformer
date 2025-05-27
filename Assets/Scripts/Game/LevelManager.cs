using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheGame
{
    public class LevelManager : PersistentSingleton<LevelManager>
    {
        public string SaveName = "SaveData.json";
        public LevelPresetsData LevelPresets;
        public List<Level> RuntimeLevels = new List<Level>();

        private string m_SaveFilePath;

        protected override void Awake()
        {
            if (!HasInstance)
            {
                
            }
            base.Awake();
        }

        protected void Start()
        {
            LoadSaveData();
        }

        public void LoadSaveData()
        {
            m_SaveFilePath = System.IO.Path.Combine(Application.persistentDataPath, SaveName);

            if (LevelPresets == null)
            {
                Debug.LogError("LevelPresets is not assigned in LevelManager. Please assign it in the inspector.");
                return;
            }

            var m_Saves = new List<LevelSaveData>();
            if (System.IO.File.Exists(m_SaveFilePath))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(m_SaveFilePath);
                    GameSaveData gameSaveData = JsonUtility.FromJson<GameSaveData>(json);
                    m_Saves = gameSaveData.Levels ?? m_Saves;
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
                LevelSaveData saveData = m_Saves.FirstOrDefault(s => s.ID == level.ID);
                if (saveData != null)
                {
                    level.Opened = saveData.Opened;
                    level.Completed = saveData.Completed;

                    if (saveData.Challenges != null)
                    {
                        level.Challenges.ForEach(challenge =>
                        {
                            ChallengeSaveData challengeSaveData = saveData.Challenges.FirstOrDefault(c => c.ID == challenge.ID);
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

        public void Save()
        {
            if (RuntimeLevels == null || RuntimeLevels.Count == 0)
            {
                Debug.LogWarning("No data to save.)");
                return;
            }

            List<LevelSaveData> saveData = RuntimeLevels.Select(level => new LevelSaveData
            {
                ID = level.ID,
                Opened = level.Opened,
                Completed = level.Completed,
                Challenges = level.Challenges.Select(challenge => challenge.GetSaveData()).ToList()
            }).ToList();

            GameSaveData gameSaveData = new GameSaveData
            {
                Levels = saveData
            };

            string json = JsonUtility.ToJson(gameSaveData);

            System.IO.File.WriteAllText(m_SaveFilePath, json);
        }
    }
}

