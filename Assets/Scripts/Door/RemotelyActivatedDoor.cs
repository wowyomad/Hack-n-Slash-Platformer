using TheGame;
using UnityEngine;
public class RemotelyActivatedDoor : Door, IActivatable
{
    [Range(1, 10)]
    [SerializeField] private int m_ActivationsRequired = 1;
    private int m_ActivationsCount = 0;
    public void Activate()
    {
        m_ActivationsCount++;
        if (m_ActivationsCount >= m_ActivationsRequired)
        {
            Open();
        }

    }

    public void Deactivate()
    {
        m_ActivationsCount = Mathf.Max(0, --m_ActivationsCount);
        if (m_ActivationsCount < m_ActivationsRequired)
        {
            Close();
        }
    }
}
