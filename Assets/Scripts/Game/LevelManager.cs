using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace TheGame
{
    public class LevelManager : MonoBehaviour
    {
        public bool IsLevelActive => CurrentLevel != null;
        public string SaveName = "SaveData.json";
        public GameDataPreset LevelPresets;
        public List<Level> RuntimeLevels = new();
        public List<Ability> RuntimeAbilities = new();
        public GameSaveData SaveData = new();
        public Level CurrentLevel;
        private string m_SaveFilePath;
        private GameManager m_GameManager;

        private IAbility m_PLayerPassiveAbility;

        private bool m_Running = false;

        private void Awake()
        {
            m_GameManager = GetComponent<GameManager>();
            m_SaveFilePath = Path.Combine(Application.persistentDataPath, SaveName);
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
                    RemovePlayerPassiveAbility();

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
                    Save();
                }
                else if (CurrentLevel.LevelStatus == Level.Status.Failed)
                {
                    m_Running = false;
                    Save();
                }
            }
        }

        public void SetPlayerPassiveAbility(IAbility ability)
        {
            if (m_Running)
            {
                Debug.LogWarning("Cannot set player ability while a level is running.");
                return;
            }
            m_PLayerPassiveAbility = ability;
        }

        public void ApplyPlayerPassiveAbility()
        {
            Player player = FindAnyObjectByType<Player>();
            if (player != null && m_PLayerPassiveAbility != null)
            {
                m_PLayerPassiveAbility.Apply(player);
            }
        }

        public void RemovePlayerPassiveAbility()
        {
            Player player = FindAnyObjectByType<Player>();
            if (player != null && m_PLayerPassiveAbility != null)
            {
                m_PLayerPassiveAbility.Remove(player);
            }
        }

        private void OnEnable()
        {
            m_GameManager.LoadCompleted += ApplyPlayerPassiveAbility;
            m_GameManager.LoadCompleted += StartLevel;
        }

        private void OnDisable()
        {
            m_GameManager.LoadCompleted -= ApplyPlayerPassiveAbility;
            m_GameManager.LoadCompleted -= StartLevel;
        }

        public void InitializeLoadSaveData()
        {
            if (LevelPresets == null)
            {
                Debug.LogError("LevelPresets is not assigned in LevelManager. Please assign it in the inspector.");
                return;
            }

            InstantiateRuntimeObjectsFromPresets();
            LoadSaveFile();
            ValidateSaveDataAgainstPresets();
            ApplySaveDataToRuntimeLevels();
            ApplySaveDataToRuntimeAbilities();
            ResolveLevelDependencies();
            
            SynchronizeProgressionState(); 
        }

        private void InstantiateRuntimeObjectsFromPresets()
        {
            RuntimeLevels = LevelPresets.Levels?.Select(Instantiate).ToList() ?? new List<Level>();
            RuntimeAbilities = LevelPresets.Abilities?.Select(Instantiate).ToList() ?? new List<Ability>();

            RuntimeLevels.ForEach(level =>
            {
                var instantiatedChallenges = level.Challenges?.Select(Instantiate).ToList() ?? new List<Challenge>();
                level.Challenges = instantiatedChallenges;

                level.Challenges.ForEach(challenge =>
                {
                    if (challenge.RewardAbility != null)
                    {
                        Ability presetRewardAbility = challenge.RewardAbility;
                        Ability runtimeRewardAbilityInstance = RuntimeAbilities.FirstOrDefault(a => a.ID == presetRewardAbility.ID);

                        if (runtimeRewardAbilityInstance != null)
                        {
                            challenge.RewardAbility = runtimeRewardAbilityInstance;
                        }
                        else
                        {
                            Debug.LogWarning($"Challenge '{challenge.Name}' in level '{level.Name}' references a RewardAbility with ID '{presetRewardAbility.ID}' that was not found in RuntimeAbilities. Reward will not be linked.");
                        }
                    }
                });
            });
        }

        private void LoadSaveFile()
        {
            if (File.Exists(m_SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(m_SaveFilePath);
                    SaveData = JsonUtility.FromJson<GameSaveData>(json) ?? new GameSaveData();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load save data from {m_SaveFilePath}: {e.Message}. Initializing new SaveData.");
                    SaveData = new GameSaveData();
                }
            }
            else
            {
                SaveData = new GameSaveData();
            }
        }

        private void ValidateSaveDataAgainstPresets()
        {
            //Levels
            if (SaveData.Levels != null && LevelPresets.Levels != null)
            {
                SaveData.Levels.RemoveAll(levelSave => levelSave.ID == null || !LevelPresets.Levels.Any(preset => preset.ID == levelSave.ID));
            }
            else
            {
                SaveData.Levels = new List<LevelSaveData>();
            }

            if (SaveData.Abilities != null && LevelPresets.Abilities != null)
            {
                 SaveData.Abilities.RemoveAll(abilitySave => abilitySave.ID == null || !LevelPresets.Abilities.Any(preset => preset.ID == abilitySave.ID));
            }
            else
            {
                SaveData.Abilities = new List<AbilitySaveData>();
            }
            
            // Ensure LastUnfinishedLevelID is valid
            if (!string.IsNullOrEmpty(SaveData.LastUnfinishedLevelID) && !RuntimeLevels.Any(l => l.ID == SaveData.LastUnfinishedLevelID))
            {
                SaveData.LastUnfinishedLevelID = string.Empty;
            }
            if (string.IsNullOrEmpty(SaveData.LastUnfinishedLevelID) && RuntimeLevels.Any())
            {
                 SaveData.LastUnfinishedLevelID = RuntimeLevels.First().ID;
            }
        }
        
        private void ApplySaveDataToRuntimeLevels()
        {
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
                            challenge.Initialize(challengeSaveData); // Initialize will handle null saveData
                            if (challengeSaveData == null)
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

        private void ApplySaveDataToRuntimeAbilities()
        {
            RuntimeAbilities.ForEach(ability =>
            {
                AbilitySaveData abilitySaveData = SaveData.Abilities.FirstOrDefault(s => s.ID == ability.ID);
                if (abilitySaveData != null)
                {
                    ability.Unlocked = abilitySaveData.Unlocked;
                }
                else
                {
                     Debug.LogWarning($"Ability with ID '{ability.ID}' found in presets but not in save data. Using default unlocked status.");
                }
            });
        }

        private void ResolveLevelDependencies()
        {
            RuntimeLevels.ForEach(level =>
            {
                var originalNextLevelPresets = level.NextLevels;
                level.NextLevels = originalNextLevelPresets?
                    .Select(nextLevelPreset => RuntimeLevels.SingleOrDefault(l => nextLevelPreset != null && l.ID == nextLevelPreset.ID))
                    .Where(resolved => resolved != null)
                    .ToList() ?? new List<Level>();

                if (originalNextLevelPresets != null && level.NextLevels.Count != originalNextLevelPresets.Count(nlp => nlp != null && RuntimeLevels.Any(rl => rl.ID == nlp.ID)))
                {
                     Debug.LogWarning(
                        $"Some next levels in \"{level.Name}\" could not be fully resolved.\n" +
                        $"Preset contains: [{string.Join(", ", originalNextLevelPresets.Where(l => l != null).Select(l => $"\"{l.Name}\""))}]\n" +
                        $"Resolved contains: [{string.Join(", ", level.NextLevels.Select(l => $"\"{l.Name}\""))}]"
                    );
                }
            });
        }

        private void RefreshAbilityUnlocksFromChallenges()
        {
            RuntimeLevels.ForEach(level =>
                level.Challenges
                    .Where(ch => ch.Status == ChallengeStatus.Complete && ch.RewardAbility != null)
                    .ToList() 
                    .ForEach(ch =>
                    {
                        // ch.RewardAbility should now be the runtime instance
                        Ability abilityToUnlock = ch.RewardAbility; 
                        if (abilityToUnlock != null)
                        {
                            if (!abilityToUnlock.Unlocked)
                            {
                                abilityToUnlock.Unlocked = true;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Challenge '{ch.Name}' in level '{level.Name}' has a null RewardAbility reference during refresh, despite being complete. Linking might have failed or preset was misconfigured.");
                        }
                    })
            );
        }

        public void SynchronizeProgressionState()
        {
            RuntimeLevels.ForEach(level =>
            {
                if (level.Completed)
                {
                    level.NextLevels.ForEach(nextLevel =>
                    {
                        if (nextLevel != null && !nextLevel.Opened)
                        {
                            nextLevel.Opened = true;
                        }
                    });
                }
            });

            RefreshAbilityUnlocksFromChallenges(); 

            Debug.Log("Game progression state synchronized.");
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

        public void UnloadCurrentLevel()
        {
            if (CurrentLevel == null)
            {
                Debug.LogWarning("No current level to unload.");
                return;
            }
            CurrentLevel.OnLevelExited();
            m_GameManager.UnloadGameLevel(CurrentLevel.SceneName);
            m_GameManager.ResetTimeScale();
            CurrentLevel = null;
            m_Running = false;
        }

        public void LoadLastUnfinishedLevel()
        {
            if (string.IsNullOrEmpty(SaveData.LastUnfinishedLevelID) && RuntimeLevels.Any())
            {
                 SaveData.LastUnfinishedLevelID = RuntimeLevels.First().ID;
            }

            if (!string.IsNullOrEmpty(SaveData.LastUnfinishedLevelID))
            {
                LoadLevelByID(SaveData.LastUnfinishedLevelID);
            }
            else
            {
                Debug.LogError("Cannot load last unfinished level: No LastUnfinishedLevelID set and no levels available.");
            }
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
            SynchronizeProgressionState(); 

            if (RuntimeLevels == null) RuntimeLevels = new List<Level>();
            if (RuntimeAbilities == null) RuntimeAbilities = new List<Ability>();

            SaveData.Levels = RuntimeLevels.Select(level => new LevelSaveData
            {
                ID = level.ID,
                Opened = level.Opened,
                Completed = level.Completed,
                Challenges = level.Challenges.Select(challenge => challenge.GetSaveData()).ToList()
            }).ToList();

            SaveData.Abilities = RuntimeAbilities.Select(ability => new AbilitySaveData
            {
                ID = ability.ID,
                Unlocked = ability.Unlocked
            }).ToList();
            
            SaveData.LastUnfinishedLevelID = CurrentLevel?.ID ?? SaveData.LastUnfinishedLevelID ?? (RuntimeLevels.Any() ? RuntimeLevels.First().ID : string.Empty);

            try
            {
                string json = JsonUtility.ToJson(SaveData, true);
                File.WriteAllText(m_SaveFilePath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save data to {m_SaveFilePath}: {e.Message}");
            }
        }

        private void StartLevel()
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
                Debug.LogWarning("No current level set when scene load completed, or scene name mismatch.");
            }
        }
    }
}