using System.Collections.Generic;

namespace TheGame
{
    [System.Serializable]
    public class LevelSaveData
    {
        public string ID;
        public bool Opened;
        public bool Completed;
        public List<ChallengeSaveData> Challenges = new List<ChallengeSaveData>();
    }
}
