using UnityEngine;

namespace TheGame
{

    [RequireComponent(typeof(Collider2D))]
    public class PickableThrowable : MonoBehaviour, IPickupable<IThrowable>
    {
        [SerializeField]
        private GameObject m_ThrowablePrefab;
        private SpriteRenderer m_SpriteRenderer;

        private void Awake()
        {
            m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (m_SpriteRenderer == null)
            {
                Debug.LogError("PickableThrowable requires a SpriteRenderer in its children.", this);
            }
            else if (m_ThrowablePrefab != null)
            {
                var throwable = m_ThrowablePrefab.GetComponent<IThrowable>();
                if (throwable != null)
                {
                    m_SpriteRenderer.sprite = throwable.Icon;
                }
                else
                {
                    Debug.LogError("Throwable Prefab does not have an IThrowable component.", m_ThrowablePrefab);
                }
            }
        }
        private void Start()
        {
            if (m_ThrowablePrefab == null)
            {
                Debug.LogError("Throwable Prefab is not assigned in PickableThrowable.", this);
            }
        }

        public IThrowable Pickup(IPicker<IThrowable> picker)
        {
            if (m_ThrowablePrefab == null)
            {
                Debug.LogError("Cannot pick up: Throwable Prefab is not assigned.", this);
                return null;
            }

            GameObject throwableInstance = Instantiate(m_ThrowablePrefab);
            IThrowable throwable = throwableInstance.GetComponent<IThrowable>();

            if (throwable == null)
            {
                Debug.LogError("Instantiated prefab does not have an IThrowable component.", throwableInstance);
                Destroy(throwableInstance);
                return null;
            }

            if (throwable is IThrowableWithThrower throwableWithThrower && picker is MonoBehaviour pickerMonoBehaviour)
            {
                throwableWithThrower.SetThrower(pickerMonoBehaviour.gameObject);
            }

            Destroy(gameObject);
            return throwable;
        }
    }
}