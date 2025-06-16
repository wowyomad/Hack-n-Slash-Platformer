using TMPro;
using UnityEngine;

public class DescriptionPopup : MonoBehaviour
{
    public string Text
    {
        get { return m_Text; }
        set
        {
            m_Text = value;
            m_TextGUI.text = m_Text;
        }
    }
    private string m_Text;

    private GameObject m_Child;
    private TextMeshProUGUI m_TextGUI;

    private bool m_Visible = false;

    [SerializeField]
    private Vector3 m_MouseOffset = new Vector3(0, 0, 0);
    private Camera m_Camera;

    private void Awake()
    {
        m_Child = transform.GetChild(0).gameObject;
        m_TextGUI = m_Child.GetComponentInChildren<TextMeshProUGUI>();
        m_Camera = Camera.main;

        m_Child.SetActive(false);
    }
    void Start()
    {
        m_TextGUI.text = m_Text;
    }

    private void Update()
    {
        if (m_Visible)
        {
            Vector3 mousePosition = Input.mousePosition + m_MouseOffset;
            RectTransform rectTransform = m_Child.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(mousePosition.x, mousePosition.y);
        }
    }

    public void Show()
    {
        m_Child.SetActive(true);
        m_Visible = true;
    }

    public void Hide()
    {
        m_Child.SetActive(false);
        m_Visible = false;
    }




}
