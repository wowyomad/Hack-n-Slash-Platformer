using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheGame
{
    [System.Serializable]
    public class GameSaveData
    {
        public List<LevelSaveData> Levels;
    }

    [System.Serializable]
    public class LevelSaveData
    {
        public string ID;
        public bool Opened;
        public bool Completed;
        public List<ChallengeSaveData> Challenges = new List<ChallengeSaveData>();
    }
    [System.Serializable]
    public class Level : ScriptableObject
    {
        public string ID = System.Guid.NewGuid().ToString();
        public string Name;
        public string Description;
        public bool Opened;
        public bool Completed;
        public string SceneName;
#if UNITY_EDITOR
        public SceneAsset SceneReference;
#endif
        public List<Challenge> Challenges = new List<Challenge>();
        public List<Level> NextLevels = new List<Level>();

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ID))
            {
                ID = System.Guid.NewGuid().ToString();
            }
            if (SceneReference != null)
            {
                SceneName = AssetDatabase.GetAssetPath(SceneReference);
                SceneName = System.IO.Path.GetFileNameWithoutExtension(SceneName);
            }
        }
    }
}
