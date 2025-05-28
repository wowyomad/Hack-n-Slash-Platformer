using TheGame;
using UnityEngine;

public class Handler_StartMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject LevelListContent;
    public GameObject ChallengeListContent;
    public GameObject LevelName;
    public GameObject LevelDescription;


    [Header("Data")]
    public LevelPresetsData LevelPresetsData;
    public LevelSaveData LevelSaveData;


    [Header("UI Prefabs")]
    public GameObject LevelButtonPrefab;
    public GameObject ChallengePrefab;

    private void Awake()
    {
        if (LevelListContent == null || ChallengeListContent == null || LevelName == null || LevelDescription == null)
        {
            Debug.LogError("UI elements are not assigned in the Handler_StartMenu script.");
        }

        if (LevelButtonPrefab == null || ChallengePrefab == null)
        {
            Debug.LogError("UI prefabs are not assigned in the Handler_StartMenu script.");
        }

        if (LevelPresetsData == null || LevelSaveData == null)
        {
            Debug.LogError("Data objects are not assigned in the Handler_StartMenu script.");
        }
    }

    private void Start()
    {
        PopulateLevelList();
        PopulateChallengeList();
    }

    private void PopulateLevelList()
    {

    }

    private void PopulateChallengeList()
    {
        
    }
}
