using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
        public GameObject LevelTogglePrefab;
        public GameObject ChallengePrefab;

        [Header("UI Variables")]
        public Color DefaultChallengeColor;
        public Color CompleteChallengeColor;


        private LevelManager m_LevelManager;
        private List<Level> m_Levels;
        private List<GameObject> m_LevelToggles;
        public Toggle CurrentLevelToggle => m_SelectedLevelIndex >= 0 && m_SelectedLevelIndex < m_LevelToggles.Count ? m_LevelToggles[m_SelectedLevelIndex].GetComponent<Toggle>() : null;
        public Level SelectedLevel => m_SelectedLevelIndex >= 0 && m_SelectedLevelIndex < m_Levels.Count ? m_Levels[m_SelectedLevelIndex] : null;
        private int m_SelectedLevelIndex = 0;
        private int m_PrevSelectedLevelIndex = -1;



        private void Awake()
        {
            if (LevelListContentRef == null || ChallengeListContentRef == null || LevelNameRef == null || LevelDescriptionRef == null)
            {
                Debug.LogError("UI elements are not assigned in the Handler_StartMenu script.");
            }

            if (LevelTogglePrefab == null || ChallengePrefab == null)
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
            m_Levels = m_LevelManager.RuntimeLevels;

            PopulateLevelList();

        }

        public void RefreshState()
        {
            for (int i = 0; i < m_Levels.Count; i++)
            {
                Level level = m_Levels[i];
                Toggle levelToggle = m_LevelToggles[i].GetComponent<Toggle>();
                levelToggle.interactable = level.Opened;
            }
            RefreshChallenges();
        }

        public void StartSelectedLevel()
        {
            if (SelectedLevel != null)
            {
                m_LevelManager.LoadLevel(SelectedLevel);
            }
        }

        private void PopulateLevelList()
        {
            if (m_Levels == null || m_Levels.Count == 0)
            {
                Debug.LogWarning("No levels available to populate the level list.");
                return;
            }

            ToggleGroup levelToggleGroup = LevelListContentRef.GetComponent<ToggleGroup>();

            m_LevelToggles = m_Levels.Select(level =>
                {
                    GameObject levelButtonGameObject = Instantiate(LevelTogglePrefab, LevelListContentRef.transform);
                    int levelIndex = m_Levels.IndexOf(level);

                    TMP_Text levelButtonText = levelButtonGameObject.GetComponentInChildren<TMP_Text>();
                    levelButtonText.text = level.Name;

                    Toggle levelToggle = levelButtonGameObject.GetComponent<Toggle>();
                    levelToggle.onValueChanged.AddListener(isOn =>
                    {
                        if (isOn)
                        {
                            SelectLevel(levelIndex);
                        }
                    });
                    levelToggle.interactable = level.Opened;
                    levelToggle.group = levelToggleGroup;
                    return levelButtonGameObject;

                })
                .ToList();

            m_SelectedLevelIndex = 0;
        }

        private void SelectLevel(int index)
        {
            if (index == m_PrevSelectedLevelIndex)
            {
                return;
            }

            if (index < 0 || index >= m_Levels.Count)
            {
                Debug.LogError($"Invalid level index: {index}. Cannot select level.");
                return;
            }

            m_SelectedLevelIndex = index;

            Level selectedLevel = m_Levels[m_SelectedLevelIndex];
            LevelNameRef.GetComponent<TMP_Text>().text = selectedLevel.Name;
            LevelDescriptionRef.GetComponent<TMP_Text>().text = selectedLevel.Description;

            RefreshChallenges();

            m_PrevSelectedLevelIndex = m_SelectedLevelIndex;
        }

        private void RefreshChallenges()
        {
            Level selectedLevel = m_Levels[m_SelectedLevelIndex];

            // Clear existing challenge list
            foreach (Transform child in ChallengeListContentRef.transform)
            {
                Destroy(child.gameObject);
            }

            selectedLevel.Challenges.ForEach(challenge =>
            {
                GameObject challengeGameObject = Instantiate(ChallengePrefab, ChallengeListContentRef.transform);

                TMP_Text challengeNumberText = challengeGameObject.transform.GetChild(0).GetComponent<TMP_Text>();
                challengeNumberText.text = selectedLevel.Challenges.IndexOf(challenge) + 1 + ".";
                challengeNumberText.color = challenge.Status == ChallengeStatus.Complete ? CompleteChallengeColor : DefaultChallengeColor;

                TMP_Text challengeDescriptionText = challengeGameObject.transform.GetChild(1).GetComponent<TMP_Text>();
                challengeDescriptionText.text = challenge.Description;
                challengeDescriptionText.color = challenge.Status == ChallengeStatus.Complete ? CompleteChallengeColor : DefaultChallengeColor;


                if (challenge.RewardAbility != null)
                {
                    Image rewardImage = challengeGameObject.transform.GetChild(2).GetComponent<Image>();

                    //if there's background?
                    if (rewardImage.transform.childCount > 0)
                    {
                        rewardImage = rewardImage.transform.GetChild(0).GetComponent<Image>();
                    }

                    rewardImage.sprite = challenge.RewardAbility.ItemIcon; ;
                }
            });
        }
    }
}
