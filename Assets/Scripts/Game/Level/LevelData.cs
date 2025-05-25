using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheGame
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Levels/Level Data")]
    public class LevelData : ScriptableObject
    {
        public string ID => m_ID;
        public string Name => m_Name;
        public Scene SceneReference => m_SceneReference;
        public List<Challenge> Challenges => m_Challenges;
        public List<LevelData> NextLevels => m_NextLevels;

        [SerializeField] private string m_ID;
        [SerializeField] private string m_Name;
        [SerializeField] private Scene m_SceneReference;
        [SerializeField] private List<Challenge> m_Challenges = new List<Challenge>();
        [SerializeField] private List<LevelData> m_NextLevels = new List<LevelData>();

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(m_ID))
            {
                m_ID = Guid.NewGuid().ToString();
            }
        }
    }

    [System.Serializable]
    public class LevelSaveData
    {
        public string ID;
        public bool Opened;
        public bool Completed;
        public List<ChallengeSaveData> Challenges = new List<ChallengeSaveData>();
    }

}