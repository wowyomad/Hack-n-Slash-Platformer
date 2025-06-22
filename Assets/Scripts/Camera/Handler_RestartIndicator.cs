using TheGame;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Handler_RestartIndicator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private UIManager m_UIManager;

    [SerializeField] private Color m_TargetColor;
    [SerializeField] private Color m_StartingColor;
    [SerializeField] private Image m_FillBar;
    [SerializeField] private bool m_FillExponentially = true;
    private GameObject m_View;

    private float MaxValue => m_UIManager.RestartDelay;
    private float CurrentValue => m_UIManager.RestartTime;

    
    void Awake()
    {
        m_UIManager = FindAnyObjectByType<UIManager>();
        m_View = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        m_View.SetActive(false);
    }

    private void Update()
    {
        if (CurrentValue <= 0.0f && m_View.activeSelf)
        {
            m_View.SetActive(false);
            return;
        }
        else if (CurrentValue > 0.0f && !m_View.activeSelf)
        {
            m_View.SetActive(true);
        }

        float t = Mathf.Clamp01(CurrentValue / MaxValue);
        if (m_FillExponentially)
        {
            t = Mathf.Pow(t, 2.0f);
        }
        m_FillBar.fillAmount = t;
        m_FillBar.color = Color.Lerp(m_StartingColor, m_TargetColor, t);
    }
}
