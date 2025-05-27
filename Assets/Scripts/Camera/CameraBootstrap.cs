using TheGame;
using UnityEngine;

public class CameraBootstrap : MonoBehaviour
{
    [SerializeField] private Collider2D m_LevelBounds;
    private CameraBehaviour m_CameraBehaviour;
    public void Initialize()
    {
        m_CameraBehaviour = FindFirstObjectByType<CameraBehaviour>();
        if (m_CameraBehaviour != null)
        {
                m_CameraBehaviour.LevelBounds = m_LevelBounds;
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                m_CameraBehaviour.FollowTarget = player.transform;
            }
            else
            {
                Debug.LogError("Player not found in the scene. Camera will not follow any target.", this);
            }
        }
    }

    void OnDestroy()
    {
        if (m_CameraBehaviour != null)
        {
            m_CameraBehaviour.LevelBounds = null;
            m_CameraBehaviour.FollowTarget = null;
        }
    }
}
