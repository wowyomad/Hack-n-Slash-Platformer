using System.Collections.Generic;
using UnityEngine;

namespace TheGame
{
    [CreateAssetMenu(fileName = "LevelPresetsData", menuName = "Game/Level Presets Data")]
    public class LevelPresetsData : ScriptableObject
    {
        public List<Level> Levels = new List<Level>();
    }
}

