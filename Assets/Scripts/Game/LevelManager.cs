using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TheGame
{
    public class LevelManager : PersistentSingleton<LevelManager>
    {
        [SerializeField] private string m_SaveName = "GameSave.json";
        [SerializeField] private List<LevelData> m_Levels;
        private LevelData m_CurrentLevel;
        private GameSaveData m_GameSaveData;
        private string m_SavePath;

        protected override void Awake()
        {
            base.Awake();
            m_SavePath = Path.Combine(Application.persistentDataPath, m_SaveName);
        }

        private void Start()
        {

        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

    }
}
