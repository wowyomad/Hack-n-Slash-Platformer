using TheGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChallengePopup : MonoBehaviour
{
    [SerializeField] private float m_PopupDuration = 4.0f;
    [SerializeField] private GameObject m_PopupContent;
    [SerializeField] private Color m_FailColor = Color.red;
    [SerializeField] private Color m_CompleteColor = Color.green; 

    private float m_Timer = 0f;
    private bool m_IsPopupVisible = false;

    private void Awake()
    {
        if (m_PopupContent == null)
        {
            Debug.LogError("Popup content is not assigned in the ChallengePopup script.");
        }
        else
        {
            m_PopupContent.SetActive(false);
        }
    }

    public void ShowPopup(Challenge challenge, bool success)
    {
        m_PopupContent.SetActive(true);
        m_Timer = 0f;
        m_IsPopupVisible = true;

        if (success)
        {
            m_PopupContent.GetComponent<Image>().color = m_CompleteColor;
            m_PopupContent.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Challenge completed: {challenge.Name}";
        }
        else
        {
            m_PopupContent.GetComponent<Image>().color = m_FailColor;
            m_PopupContent.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Challenge failed: {challenge.Name}";
        }
    }

    public void HidePopup()
    {
        m_PopupContent.SetActive(false);
        m_IsPopupVisible = false;
    }

    private void Update()
    {
        if (m_IsPopupVisible)
        {
            m_Timer += Time.deltaTime;
            if (m_Timer >= m_PopupDuration)
            {
                HidePopup();
            }
        }
    }
}
