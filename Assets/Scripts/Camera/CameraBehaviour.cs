using GameActions;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public CameraBehaviourSettings Settings;
    public Transform FollowTarget;
    public InputReader Input;

    [Min(1.0f)]
    public float OffsetMultiplier = 1.0f;

    public float CameraHeight => m_MainCamera.orthographic ? m_MainCamera.orthographicSize * 2f : 0f;
    public float CameraWidth => CameraHeight * m_MainCamera.aspect;

    private Camera m_MainCamera;
    private Transform m_Target;

    float m_OriginalOrthographicSize;
    [SerializeField] private float m_ZoomLevel = 1.0f;
    private Vector2 m_CursorScreenPosition => Input.CursorPosition;

    private Vector3 m_FollowVelocity;
    private Vector2 m_MouseOffsetVelocity;
    private Vector2 m_CurrentMouseOffset;
    private Vector3 m_FollowTargetPreviousPosition;
    private Vector3 m_CameraLeadOffset;
    private Vector3 m_CameraLeadVelocity;

    private void Awake()
    {
        m_MainCamera = GetComponentInChildren<Camera>();
        m_Target = transform;
        if (FollowTarget != null)
        {
            m_FollowTargetPreviousPosition = FollowTarget.position;
        }
        else
        {
            Debug.LogError("CameraBehaviour: FollowTarget is not assigned in Awake!", this);
        }

        if (Input == null) Debug.LogError("CameraBehaviour: InputReader is not assigned!", this);
        if (Settings == null) Debug.LogError("CameraBehaviour: Settings are not assigned!", this);
        if (m_MainCamera == null) Debug.LogError("CameraBehaviour: Camera component not found!", this);
    }

    private void Start()
    {
        m_OriginalOrthographicSize = m_MainCamera.orthographicSize;
    }

    private void OnEnable()
    {
        Input.ListenEvents(this);    
    }

    private void OnDisable()
    {
        Input.StopListening(this);
    }


    private void LateUpdate()
    {
        if (FollowTarget == null || Settings == null || Input == null || m_MainCamera == null || !m_MainCamera.orthographic)
        {
            if (m_MainCamera != null && !m_MainCamera.orthographic) Debug.LogWarning("CameraBehaviour requires an Orthographic Camera.", this);
            return;
        }

        UpdateMouseOffset();
        UpdateZoom();

        Vector3 currentFollowTargetPosition = FollowTarget.position;
        Vector3 velocity = Time.deltaTime > Mathf.Epsilon
                         ? (currentFollowTargetPosition - m_FollowTargetPreviousPosition) / Time.deltaTime
                         : Vector3.zero;
        m_FollowTargetPreviousPosition = currentFollowTargetPosition;

        Vector3 baseTargetLeadOffset = new Vector3(velocity.x * Settings.LeadFactor.x, velocity.y * Settings.LeadFactor.y, 0f);
        Vector3 scaledTargetLeadOffset = baseTargetLeadOffset * OffsetMultiplier;
        m_CameraLeadOffset = Vector3.SmoothDamp(m_CameraLeadOffset, scaledTargetLeadOffset, ref m_CameraLeadVelocity, Settings.LeadSmoothTime);

        Vector3 targetPosition = new Vector3(
            FollowTarget.position.x + Settings.DefaultOffset.x + m_CurrentMouseOffset.x + m_CameraLeadOffset.x,
            FollowTarget.position.y + Settings.DefaultOffset.y + m_CurrentMouseOffset.y + m_CameraLeadOffset.y,
            m_Target.position.z
        );

        Vector3 currentPosition = m_Target.position;

        Vector3 smoothedPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref m_FollowVelocity, Settings.FollowSmoothTime);

        m_Target.position = new Vector3(smoothedPosition.x, smoothedPosition.y, m_Target.position.z);
    }

    private void UpdateMouseOffset()
    {
        if (m_MainCamera == null || Settings == null || m_Target == null) return;

        float halfHeight = CameraHeight / 2.0f;
        float halfWidth = CameraWidth / 2.0f;

        if (halfWidth <= Mathf.Epsilon || halfHeight <= Mathf.Epsilon)
        {
            m_CurrentMouseOffset = Vector2.SmoothDamp(m_CurrentMouseOffset, Vector2.zero, ref m_MouseOffsetVelocity, Settings.MouseInfluenceSmoothTime);
            return;
        }

        float zDistance = Mathf.Abs(m_Target.position.z);
        if (zDistance < Mathf.Epsilon) zDistance = 0.1f;

        Vector3 mouseWorldPosition = m_MainCamera.ScreenToWorldPoint(new Vector3(m_CursorScreenPosition.x, m_CursorScreenPosition.y, zDistance));
        Vector3 screenCenterWorldPosition = m_MainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, zDistance));

        Vector2 rawOffsetWorld = new Vector2(mouseWorldPosition.x - screenCenterWorldPosition.x, mouseWorldPosition.y - screenCenterWorldPosition.y);
        Vector2 normalizedMouseOffset = new Vector2(rawOffsetWorld.x / halfWidth, rawOffsetWorld.y / halfHeight);

        Vector3 viewportPoint = m_MainCamera.ScreenToViewportPoint(new Vector3(m_CursorScreenPosition.x, m_CursorScreenPosition.y, zDistance));
        if (Settings.ResetMouseOffsetOutsideBounds && (viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1))
        {
            m_CurrentMouseOffset = Vector2.SmoothDamp(m_CurrentMouseOffset, Vector2.zero, ref m_MouseOffsetVelocity, Settings.MouseInfluenceSmoothTime);
            return;
        }

        Vector2 targetOffsetWorld = Vector2.zero;

        float centerRadiusSqr = Settings.CenterRadius * Settings.CenterRadius;
        if (normalizedMouseOffset.sqrMagnitude > centerRadiusSqr)
        {
            float influenceRange = 1.0f - Settings.CenterRadius;

            if (influenceRange > Mathf.Epsilon)
            {
                float influenceFactorX = Mathf.InverseLerp(Settings.CenterRadius, 1.0f, Mathf.Abs(normalizedMouseOffset.x));
                float influenceFactorY = Mathf.InverseLerp(Settings.CenterRadius, 1.0f, Mathf.Abs(normalizedMouseOffset.y));

                Vector2 targetOffsetNormalizedInfluence = new Vector2(Mathf.Sign(normalizedMouseOffset.x) * influenceFactorX,
                                                                    Mathf.Sign(normalizedMouseOffset.y) * influenceFactorY);

                Vector2 baseTargetOffsetWorld = new Vector2(
                    targetOffsetNormalizedInfluence.x * Settings.MaxMouseOffsetX * halfWidth,
                    targetOffsetNormalizedInfluence.y * Settings.MaxMouseOffsetY * halfHeight
                );

                targetOffsetWorld = baseTargetOffsetWorld;
            }
        }

        m_CurrentMouseOffset = OffsetMultiplier * Vector2.SmoothDamp(m_CurrentMouseOffset, targetOffsetWorld, ref m_MouseOffsetVelocity, Settings.MouseInfluenceSmoothTime);
    }
    private void UpdateZoom()
    {
        if (m_MainCamera == null || !m_MainCamera.orthographic) return;

        float targetOrthographicSize = m_OriginalOrthographicSize / m_ZoomLevel;
        m_MainCamera.orthographicSize = targetOrthographicSize;
    }

    [GameAction(ActionType.Zoom)]
    private void OnZoom(float value)
    {
        m_ZoomLevel -= value * Settings.ZoomStep;
        m_ZoomLevel = Mathf.Clamp(m_ZoomLevel, Settings.MinZoom, Settings.MaxZoom);
    }
}
