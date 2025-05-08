using UnityEngine;

public class TestSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject ObjectReference;
    public int MaximumEnemyCount = 10;
    public int SpawnPerInterval = 1;
    public float SpawnInterval = 1.0f;
    public float SpawnDelay = 1.0f;
    public SpawnBounds Bounds = new SpawnBounds { Left = -5, Right = 5, Top = 10, Bottom = 0 };

    private int m_CurrentEnemyCount = 0;
    private float m_SpawnTimer = 0.0f;

    private void OnValidate()
    {
        if (ObjectReference.GetComponent<IDestroyable>() == null)
        {
            Debug.LogWarning("Spawned object must implement IDestroyable");
            ObjectReference = null;
        }
    }

    private void Start()
    {
        m_SpawnTimer = -SpawnDelay;
    }

    private void Update()
    {
        m_SpawnTimer += Time.deltaTime;
        if (m_SpawnTimer >= SpawnInterval)
        {
            m_SpawnTimer = 0.0f;
            int spawned = 0;
            while (m_CurrentEnemyCount + spawned < MaximumEnemyCount && spawned < SpawnPerInterval)
            {
                if (TrySpawnEnemy())
                {
                    spawned++;
                }
                else
                {
                    break;
                }
            }
            m_CurrentEnemyCount += spawned;
        }
    }

    private bool TrySpawnEnemy()
    {
        if (ObjectReference == null)
        {
            Debug.LogWarning("Object reference is null");
            return false;
        }

        Vector3 offset = new Vector3(Random.Range(Bounds.Left, Bounds.Right), Random.Range(Bounds.Bottom, Bounds.Top));
        GameObject spawnedObject = Instantiate(ObjectReference, transform.position + offset, Quaternion.identity);
        if (spawnedObject == null)
        {
            Debug.LogWarning("Failed to instantiate object");
            return false;
        }

        var destroyable = spawnedObject.GetComponent<IDestroyable>();
        destroyable.DestroyedEvent += OnObjectDestroyed;
        return true;
    }

    private void OnObjectDestroyed(IDestroyable destroyable)
    {
        m_CurrentEnemyCount--;
        destroyable.DestroyedEvent -= OnObjectDestroyed;
    }

    [System.Serializable]
    public struct SpawnBounds
    {
        public float Left, Right, Top, Bottom;
    }
}
