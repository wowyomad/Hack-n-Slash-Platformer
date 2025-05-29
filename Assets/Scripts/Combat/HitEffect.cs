using Unity.Profiling;
using UnityEngine;

namespace TheGame
{
    public class HitEffect : MonoBehaviour
    {
        private IHittable m_Hittable;
        private Animator m_Animator;
        private SpriteRenderer m_SpriteRenderer;

        [SerializeField] private string m_AnimationTriggerName = "Hit_";
        [SerializeField] private int m_AnimationTriggerIndexStart = 1;
        [SerializeField] private int m_AnimationTriggerIndexEnd = 3;
        [SerializeField] private Vector2 m_OffsetRange = new Vector2(0.5f, 0.5f);

        private Vector3 m_SupposedHitPosition;

        private void Awake()
        {
            m_Hittable = GetComponentInParent<IHittable>();
            m_Animator = GetComponent<Animator>();
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            m_SupposedHitPosition = transform.position;
        }

        private void OnEnable()
        {
            if (m_Hittable != null)
            {
                m_Hittable.OnHit += PlayHitAnimation;
            }
        }

        private void OnDisable()
        {
            if (m_Hittable != null)
            {
                m_Hittable.OnHit -= PlayHitAnimation;
            }
        }

        private void Update()
        {
            //Optimization!!1Адин
            if (m_SpriteRenderer.enabled)
            {
                transform.position = m_SupposedHitPosition;
            }
        }

        private void PlayHitAnimation()
        {
            if (m_Animator != null)
            {
                m_SupposedHitPosition = transform.parent.position + new Vector3(
                    Random.Range(-m_OffsetRange.x, m_OffsetRange.x),
                    Random.Range(-m_OffsetRange.y, m_OffsetRange.y),
                    0f
                );
                m_SpriteRenderer.enabled = true;
                int randomIndex = Random.Range(m_AnimationTriggerIndexStart, m_AnimationTriggerIndexEnd + 1);
                m_Animator.SetTrigger(m_AnimationTriggerName + randomIndex);
            }
            else
            {
                Debug.LogError("Animator is not assigned in HitEffect.");
            }
        }

        public void DisableHitEffect()
        {
            m_SpriteRenderer.enabled = false;
        }
    }
}
