using System.Collections.Generic;

namespace TheGame
{
    [System.Serializable]
    public class GameSaveData
    {
        public List<LevelSaveData> Levels = new();
        public List<AbilitySaveData> Abilities = new();
        public string LastUnfinishedLevelID;
    }
}
