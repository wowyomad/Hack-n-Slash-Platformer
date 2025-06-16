using TheGame;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Handler_Ability : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image m_IconSpriteRenderer;
    private Image m_CrossSpriteRenderer;

    [SerializeField] private DescriptionPopup m_DescriptionPopup;

    private Ability m_AbilityReference;
    void Awake()
    {
        var spriteRenderers = GetComponentsInChildren<Image>(true);
        if (spriteRenderers.Length < 2)
        {
            Debug.LogError("Handler_Ability requires two Image components in its children (icon and cross).", this);
        }
        else
        {
            m_IconSpriteRenderer = spriteRenderers[0];
            m_CrossSpriteRenderer = spriteRenderers[1];
        }

        //TODO: on initial load, there would be no ability reference 😨
    }

    private void OnMouseEnter()
    {
        if (m_DescriptionPopup != null && m_AbilityReference != null)
        {
            m_DescriptionPopup.Text = m_AbilityReference.Description;
            m_DescriptionPopup.Show();
        }
        else
        {
            Debug.LogError("DescriptionPopup is not assigned in Handler_Ability.", this);
        }
    }
    private void OnMouseExit()
    {
        if (m_DescriptionPopup != null && m_AbilityReference != null)
        {
            m_DescriptionPopup.Hide();
        }
        else
        {
            Debug.LogError("DescriptionPopup is not assigned in Handler_Ability.", this);
        }
    }

    public void Lock(Ability ability)
    {
        m_AbilityReference = ability;
        if (m_CrossSpriteRenderer != null)
        {
            m_CrossSpriteRenderer.enabled = true;
            m_IconSpriteRenderer.sprite = ability.ItemIcon;
        }
        else
        {
            Debug.LogError("Cross Sprite Renderer is not assigned in Handler_Ability.", this);
        }
    }
    public void Unlock(Ability ability)
    {
        m_AbilityReference = ability;
        if (m_CrossSpriteRenderer != null)
        {
            m_CrossSpriteRenderer.enabled = false;
            m_IconSpriteRenderer.sprite = ability.ItemIcon;
        }
        else
        {
            Debug.LogError("Cross Sprite Renderer is not assigned in Handler_Ability.", this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => OnMouseEnter();

    public void OnPointerExit(PointerEventData eventData) => OnMouseExit();
}
