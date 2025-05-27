using System.Collections.Generic;

namespace TheGame
{
    [System.Serializable]
    public class GameSaveData
    {
        public List<LevelSaveData> Levels;
        public string LastUnfinishedLevelID;
    }
}
