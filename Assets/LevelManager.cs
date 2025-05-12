using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject m_MainMenu;

    private void OnEnable()
    {
        EventBus<PlayerDeadEvent>.OnEvent += ShowMainMenu;
    }

    private void OnDisable()
    {
        EventBus<PlayerDeadEvent>.OnEvent -= ShowMainMenu;
    }

    private void ShowMainMenu(PlayerDeadEvent playerDeadEvent)
    {
        Debug.Log("Showing menu");
        m_MainMenu.SetActive(true);
    }


}
