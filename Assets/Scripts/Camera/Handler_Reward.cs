using TheGame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Handler_Reward : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image m_IconSpriteRenderer;
    [SerializeField] private DescriptionPopup m_DescriptionPopup;

    private Ability m_AbilityReference;

    void Awake()
    {
        m_IconSpriteRenderer = transform.GetChild(0).GetComponent<Image>();
        if (m_IconSpriteRenderer == null)
        {
            Debug.LogError("Handler_Reward requires an Image component in its children.", this);
        }

        if (m_DescriptionPopup == null)
        {
            m_DescriptionPopup = DescriptionPopup.Get();
            if (m_DescriptionPopup == null)
            {
                Debug.LogError("DescriptionPopup instance not found in Handler_Reward.", this);
            }
        }
    }

    public void SetRewardAbility(Ability ability)
    {
        if (ability == null)
        {
            Debug.LogError("Attempted to set a null ability in Handler_Reward.", this);
            return;
        }

        m_AbilityReference = ability;
        if (m_IconSpriteRenderer != null)
        {
            m_IconSpriteRenderer.sprite = ability.ItemIcon;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_DescriptionPopup != null && m_AbilityReference != null)
        {
            m_DescriptionPopup.Text = m_AbilityReference.Description;
            m_DescriptionPopup.Show();
        }
        else if (m_DescriptionPopup == null)
        {
            Debug.LogError("DescriptionPopup is not assigned in Handler_Reward.", this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (m_DescriptionPopup != null && m_AbilityReference != null)
        {
            m_DescriptionPopup.Hide();
        }
    }
}
