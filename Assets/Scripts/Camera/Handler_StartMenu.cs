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
        public GameObject SelectedItemIconRef;
        public GameObject SelectedItemLockRef;
        public GameObject NextItemButtonRef;
        public GameObject PrevItemButtonRef;



        [Header("UI Prefabs")]
        public GameObject LevelTogglePrefab;
        public GameObject ChallengePrefab;

        [Header("UI Variables")]
        public Color DefaultChallengeColor;
        public Color CompleteChallengeColor;


        [Header("Internal references")]
        public Handler_Ability AbilityHandler;


        private LevelManager m_LevelManager;
        private List<Level> m_Levels;
        private List<Ability> m_Abilities;
        private List<GameObject> m_LevelToggles;
        public Toggle CurrentLevelToggle => m_SelectedLevelIndex >= 0 && m_SelectedLevelIndex < m_LevelToggles.Count ? m_LevelToggles[m_SelectedLevelIndex].GetComponent<Toggle>() : null;
        public Level SelectedLevel => m_SelectedLevelIndex >= 0 && m_SelectedLevelIndex < m_Levels.Count ? m_Levels[m_SelectedLevelIndex] : null;
        private int m_SelectedLevelIndex = 0;
        private int m_PrevSelectedLevelIndex = -1;
        private int m_SelectedAbilityIndex = 0;


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
            m_Abilities = m_LevelManager.RuntimeAbilities;

            PopulateLevelList();

            SetPassiveAbility();

        }

        private void OnEnable()
        {
            NextItemButtonRef.GetComponent<Button>().onClick.AddListener(NextPassiveAbility);
            PrevItemButtonRef.GetComponent<Button>().onClick.AddListener(PrevPassiveAbility);
        }

        private void OnDisable()
        {
            NextItemButtonRef.GetComponent<Button>().onClick.RemoveListener(NextPassiveAbility);
            PrevItemButtonRef.GetComponent<Button>().onClick.RemoveListener(PrevPassiveAbility);
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

        private void SetPassiveAbility()
        {
            Ability selectedAbility = m_Abilities[m_SelectedAbilityIndex];
            if (selectedAbility.Unlocked)
            {
                m_LevelManager.SetPlayerPassiveAbility(selectedAbility);
                AbilityHandler.Unlock(selectedAbility);
            }
            else
            {
                m_LevelManager.SetPlayerPassiveAbility(null);
                AbilityHandler.Lock(selectedAbility);
            }
        }

        public void NextPassiveAbility()
        {
            if (m_Abilities == null || m_Abilities.Count == 0)
            {
                Debug.LogWarning("No abilities available to select.");
                return;
            }

            m_SelectedAbilityIndex = (m_SelectedAbilityIndex + 1) % m_Abilities.Count;
            SetPassiveAbility();
        }

        public void PrevPassiveAbility()
        {
            if (m_Abilities == null || m_Abilities.Count == 0)
            {
                Debug.LogWarning("No abilities available to select.");
                return;
            }
            m_SelectedAbilityIndex = (m_SelectedAbilityIndex - 1 + m_Abilities.Count) % m_Abilities.Count;
            SetPassiveAbility();
        }

        private void RefreshChallenges()
        {
            Level selectedLevel = m_Levels[m_SelectedLevelIndex];

            if (m_SelectedLevelIndex == 0)
            {

                SelectedItemIconRef.GetComponent<Image>().sprite = selectedLevel.Challenges[0].RewardAbility?.ItemIcon;
                SelectedItemLockRef.GetComponent<Image>().enabled = selectedLevel.Challenges[0].Status != ChallengeStatus.Complete;
            }

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

                var description = challengeGameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                description.text = challenge.Description;
                var themeApplier = description.GetComponent<TextThemeApplier>();

                if (challenge.Status == ChallengeStatus.Complete)
                {
                    description.fontStyle = FontStyles.Strikethrough;
                    themeApplier.ColorType = TextThemeApplier.TextColorType.GoodGlyphs;
                    themeApplier.ApplyTheme();
                }
                else
                {
                    description.fontStyle = FontStyles.Normal;
                    themeApplier.ColorType = TextThemeApplier.TextColorType.BadGlyphs;
                    themeApplier.ApplyTheme();
                }


                if (challenge.RewardAbility != null)
                {
                    Image rewardImage = challengeGameObject.transform.GetChild(2).GetComponent<Image>();

                    //if there's background?
                    if (rewardImage.transform.childCount > 0)
                    {
                        rewardImage = rewardImage.transform.GetChild(0).GetComponent<Image>();
                    }

                    rewardImage.sprite = challenge.RewardAbility.ItemIcon;
                }
            });
        }
    }
}
