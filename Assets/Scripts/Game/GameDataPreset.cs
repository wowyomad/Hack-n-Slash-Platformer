using System.Collections.Generic;
using UnityEngine;

namespace TheGame
{
    [CreateAssetMenu(fileName = "LevelPresetsData", menuName = "Game/Level Presets Data")]
    public class GameDataPreset : ScriptableObject
    {
        public List<Level> Levels = new List<Level>();
        public List<Ability> Abilities = new List<Ability>();
    }
}

