using UnityEngine;

public class PersistentSingleton<T> : MonoBehaviour where T : Component
{

    public bool HasInstance => m_Instance != null;
    private static T m_Instance;
    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindFirstObjectByType<T>();
                if (m_Instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name + " Auto-Generated Singleton");
                    obj.hideFlags = HideFlags.HideAndDontSave;
                    m_Instance = obj.AddComponent<T>();
                }
            }
            return m_Instance;
        }
    }

    protected virtual void Awake()
    {
        InitializeSingleton();
    }

    private void InitializeSingleton()
    {
        if(!Application.isPlaying)
        {
            return;
        }

        if (m_Instance != null && m_Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        m_Instance = this as T;
        DontDestroyOnLoad(gameObject);
    }
}