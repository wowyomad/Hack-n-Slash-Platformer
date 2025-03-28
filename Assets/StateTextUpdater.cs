using TMPro;
using UnityEngine;

public class StateTextUpdater : MonoBehaviour
{
    TextMeshProUGUI m_Text;
    [SerializeField] Player m_Player;
    void Awake()
    {
        m_Text = GetComponent<TextMeshProUGUI>();   
        if (m_Player == null )
        {
            m_Player = GameObject.FindWithTag("Player")?.GetComponent<Player>();
        }
    }

    private void Start()
    {
       
    }

    private void OnEnable()
    {
        if (m_Player != null)
        {
            m_Player.OnStateChange += UpdateStateText;
        }
    }
    private void OnDisable()
    {
        if (m_Player != null)
        {
            m_Player.OnStateChange -= UpdateStateText;
        }
    }

    protected void UpdateStateText(IState state)
    {
        m_Text.text = state.ToString(); 
    }
}
