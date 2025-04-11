using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class StateText : MonoBehaviour
{
    [SerializeField] private GameObject m_ObjectToTrack;
    [SerializeField] private IStateTrackable m_TrackableState;

    private TextMeshProUGUI m_Text;
    void Awake()
    {
        m_Text = GetComponent<TextMeshProUGUI>();   
        if (m_TrackableState == null)
        {
            
        }
    }

    private void OnEnable()
    {
        if (m_ObjectToTrack != null)
        {
            if(m_ObjectToTrack.TryGetComponent(out m_TrackableState))
            {
                m_TrackableState.StateChanged += UpdateStateText;
            }
        }
    }
    private void OnDisable()
    {
        if (m_TrackableState != null)
        {
            try
            {
                if (m_ObjectToTrack.TryGetComponent(out m_TrackableState))
                {
                    m_TrackableState.StateChanged -= UpdateStateText;
                }
            } catch (MissingReferenceException)
            {
                //It's OK.
                Debug.Log("Tried to unsubscribe from OnStateChange event, but the object was already destroyed.");
            }

        }
    }

    protected void UpdateStateText(IState prevoius, IState next)
    {
        m_Text.text = next.ToString(); 
    }
}
