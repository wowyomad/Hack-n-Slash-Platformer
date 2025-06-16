using UnityEngine;

namespace TheGame
{

    [RequireComponent(typeof(Collider2D))]
    public class PickableThrowable : MonoBehaviour, IPickupable<IThrowable>
    {
        [SerializeField]
        private GameObject m_ThrowablePrefab;
        private SpriteRenderer m_IconSpriteRenderer;
        private SpriteRenderer m_CrossSpriteRenderer;

        private Player m_PlayerReference;

        private void Awake()
        {
            var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            if (spriteRenderers.Length < 2)
            {
                Debug.LogError("PickableThrowable requires two SpriteRenderers in its children (icon and cross).", this);
            }
            else
            {
                m_IconSpriteRenderer = spriteRenderers[0];
                m_CrossSpriteRenderer = spriteRenderers[1];
            }

            if (m_IconSpriteRenderer != null && m_ThrowablePrefab != null)
            {
                var throwable = m_ThrowablePrefab.GetComponent<IThrowable>();
                if (throwable != null)
                {
                    m_IconSpriteRenderer.sprite = throwable.Icon;
                }
                else
                {
                    Debug.LogError("Throwable Prefab does not have an IThrowable component.", m_ThrowablePrefab);
                }
            }

            m_PlayerReference = FindAnyObjectByType<Player>();
        }
        private void Start()
        {
            if (m_ThrowablePrefab == null)
            {
                Debug.LogError("Throwable Prefab is not assigned in PickableThrowable.", this);
            }

            UpdateCrossSprite();
        }

        private void Update()
        {
            UpdateCrossSprite();
        }

        private void UpdateCrossSprite()
        {
            if (m_PlayerReference == null || m_CrossSpriteRenderer == null)
                return;

            m_CrossSpriteRenderer.enabled = !m_PlayerReference.CanPickupKnifes;
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