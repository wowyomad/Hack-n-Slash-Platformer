using System.Collections.Generic;
using System.Linq;
using TheGame;
using TMPro;
using Unity.AppUI.UI;
using UnityEngine;

namespace TheGame
{
    public class Handler_StartMenu : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject LevelListContentRef;
        public GameObject ChallengeListContentRef;
        public GameObject LevelNameRef;
        public GameObject LevelDescriptionRef;

        [Header("UI Prefabs")]
        public GameObject LevelButtonPrefab;
        public GameObject ChallengePrefab;

        private LevelManager m_LevelManager;
        private List<Level> m_Levels;
        private GameSaveData m_SaveData;
        private List<GameObject> m_LevelButtons;


        private void Awake()
        {
            if (LevelListContentRef == null || ChallengeListContentRef == null || LevelNameRef == null || LevelDescriptionRef == null)
            {
                Debug.LogError("UI elements are not assigned in the Handler_StartMenu script.");
            }

            if (LevelButtonPrefab == null || ChallengePrefab == null)
            {
                Debug.LogError("UI prefabs are not assigned in the Handler_StartMenu script.");
            }

            if (!TryGetComponent<LevelManager>(out m_LevelManager))
            {
                Debug.LogError("LevelManager component not found.");
            }
        }

        private void Start()
        {
            //Must be called after LevelManager start!
            m_SaveData = m_LevelManager.SaveData;
            m_Levels = m_LevelManager.RuntimeLevels;

            PopulateLevelList();
            PopulateChallengeList();
        }

        private void PopulateLevelList()
        {
            m_LevelButtons = m_Levels
                .Select(level =>
                {
                    GameObject levelButtonGameObject = Instantiate(LevelButtonPrefab, LevelListContentRef.transform);

                    TMP_Text levelButtonText = levelButtonGameObject.GetComponentInChildren<TMP_Text>();
                    levelButtonText.text = level.Name;

                    return levelButtonGameObject;

                })
                .ToList();
        }

        private void PopulateChallengeList()
        {

        }
    }
}
