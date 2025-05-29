using TheGame;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPickedItemIndicator : MonoBehaviour
{
    [Header("References")]
    private Player m_Player;
    private Image m_Icon;

    [Header("Blinking Animation")]
    [SerializeField] private float m_BlinkShowDuration = 0.5f;
    [SerializeField] private float m_BlinkHideDuration = 0.3f;

    private Sprite m_LastIcon;
    private float m_BlinkTimer;
    private bool m_IsBlinkPhaseShow = true; // Start by showing

    private void Awake()
    {
        m_Player = GetComponentInParent<Player>();
        m_Icon = GetComponentInChildren<Image>();

        if (m_Player == null)
        {
            Debug.LogError("PlayerPickedItemIndicator requires a Player component in its parent.", this);
        }
        if (m_Icon == null)
        {
            Debug.LogError("PlayerPickedItemIndicator requires an Image component in its children.", this);
            enabled = false; // Disable script if no icon to manage
        }
    }

    private void Start()
    {
        if (m_Player.ThrowableInHand != null)
        {
            m_LastIcon = m_Player.ThrowableInHand.Icon;
            m_Icon.sprite = m_LastIcon;
            m_Icon.enabled = true; 
        }
        else
        {
            m_Icon.enabled = false;
        }
    }

    void DisplayItemIcon()
    {
        if (m_Player == null || m_Icon == null) return;

        var currentIcon = m_Player.ThrowableInHand != null ? m_Player.ThrowableInHand.Icon : null;

        if (currentIcon == null)
        {
            if (m_Icon.enabled) // Only disable if it was enabled
            {
                m_Icon.enabled = false;
            }
            m_Icon.sprite = null;
            m_LastIcon = null; // Reset last icon
            return;
        }

        if (currentIcon != m_LastIcon)
        {
            m_Icon.sprite = currentIcon;
            m_LastIcon = currentIcon;
            // Reset blink to start with show phase when icon changes
            m_IsBlinkPhaseShow = true;
            m_BlinkTimer = 0f;
            m_Icon.enabled = true; // Ensure it's visible initially
        }

        m_BlinkTimer += Time.deltaTime;

        if (m_IsBlinkPhaseShow)
        {
            if (!m_Icon.enabled) m_Icon.enabled = true; // Ensure it's shown during show phase

            if (m_BlinkTimer >= m_BlinkShowDuration)
            {
                m_IsBlinkPhaseShow = false;
                m_BlinkTimer = 0f;
                m_Icon.enabled = false; // Start hide phase
            }
        }
        else // Blink Hide Phase
        {
            if (m_Icon.enabled) m_Icon.enabled = false; // Ensure it's hidden during hide phase

            if (m_BlinkTimer >= m_BlinkHideDuration)
            {
                m_IsBlinkPhaseShow = true;
                m_BlinkTimer = 0f;
                m_Icon.enabled = true; // Start show phase
            }
        }
    }

    void Update()
    {
        DisplayItemIcon();
    }
}
