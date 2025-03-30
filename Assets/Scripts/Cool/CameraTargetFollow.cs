using GameActions;
using UnityEngine;

public class CameraTargetFollow : MonoBehaviour
{
    public Transform Target; // The transform this script will move
    public Transform FollowTarget; // The transform to generally follow
    public InputReader Input;

    [Range(0f, 10f)]
    public float FollowSmoothTime = 0.3f;

    public float MaxMouseOffset = 3f;
    public float CenterRadius = 3f;
    [Range(0f, 10f)]
    public float MouseInfluenceSmoothTime = 0.1f;

    public Vector3 DefaultOffset = new Vector3(0f, 5f, 0f);

    public float LeadFactor = 0.5f;
    public float LeadSmoothTime = 0.2f;

    public float CameraWidth => m_MainCamera.orthographicSize * 2f * m_MainCamera.aspect;
    public float CameraHeight => m_MainCamera.orthographicSize * 2f;

    private Vector2 m_CursorScreenPosition => Input.CursorPosition;
    private Vector3 m_FollowVelocity;
    private Vector2 m_MouseOffsetVelocity;
    private Vector2 m_CurrentMouseOffset;
    private Vector3 m_FollowTargetPreviousPosition;
    private Vector3 m_CameraLeadOffset;
    private Vector3 m_CameraLeadVelocity;

    private Camera m_MainCamera;

    private void Awake()
    {
        m_MainCamera = Camera.main;

        if (Target == null)
        {
            Debug.LogError("Target transform is not assigned in CameraTargetFollow.");
            enabled = false;
        }

        if (FollowTarget != null)
        {
            m_FollowTargetPreviousPosition = FollowTarget.position;
        }
    }

    private void Update()
    {
        if (Target == null || FollowTarget == null) return;

        UpdateMouseOffset();

        Vector3 currentFollowTargetPosition = FollowTarget.position;
        Vector3 velocity = (currentFollowTargetPosition - m_FollowTargetPreviousPosition) / Time.deltaTime;
        m_FollowTargetPreviousPosition = currentFollowTargetPosition;

        Vector3 targetLeadOffset = new Vector3(-velocity.x * LeadFactor, -velocity.y * LeadFactor, 0f);
        m_CameraLeadOffset = Vector3.SmoothDamp(m_CameraLeadOffset, targetLeadOffset, ref m_CameraLeadVelocity, LeadSmoothTime);

        float originalZ = Target.position.z;
        Vector3 targetPosition = Target.position;

        Vector3 targetXY = new Vector3(FollowTarget.position.x + DefaultOffset.x + m_CurrentMouseOffset.x + m_CameraLeadOffset.x, FollowTarget.position.y + DefaultOffset.y + m_CurrentMouseOffset.y + m_CameraLeadOffset.y, originalZ);
        Vector3 currentXY = new Vector3(Target.position.x, Target.position.y, originalZ);
        Vector3 smoothedXY = Vector3.SmoothDamp(currentXY, targetXY, ref m_FollowVelocity, FollowSmoothTime);
        targetPosition = new Vector3(smoothedXY.x, smoothedXY.y, originalZ);

        Target.position = targetPosition;
    }

    private void UpdateMouseOffset()
    {
        if (Target == null || m_MainCamera == null || FollowTarget == null) return;

        Vector3 mouseWorldPosition = m_MainCamera.ScreenToWorldPoint(new Vector3(m_CursorScreenPosition.x, m_CursorScreenPosition.y, Target.position.z - m_MainCamera.transform.position.z));

        Vector3 screenCenterWorldPosition = m_MainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, Target.position.z - m_MainCamera.transform.position.z));

        Vector2 rawOffset = new Vector2(mouseWorldPosition.x - screenCenterWorldPosition.x, mouseWorldPosition.y - screenCenterWorldPosition.y);

        float mouseDistance = rawOffset.magnitude;

        Vector2 targetOffset = Vector2.zero;

        if (mouseDistance > CenterRadius)
        {
            float influenceHalfWidth = CameraWidth / 2.0f;
            float influenceHalfHeight = CameraHeight / 2.0f;

            float influenceFactorX = 0f;
            float influenceFactorY = 0f;

            float effectiveOffsetX = Mathf.Abs(rawOffset.x) - CenterRadius;
            float effectiveOffsetY = Mathf.Abs(rawOffset.y) - CenterRadius;

            if (effectiveOffsetX > 0 && influenceHalfWidth > 0)
            {
                influenceFactorX = Mathf.Clamp01(effectiveOffsetX / influenceHalfWidth);
            }

            if (effectiveOffsetY > 0 && influenceHalfHeight > 0)
            {
                influenceFactorY = Mathf.Clamp01(effectiveOffsetY / influenceHalfHeight);
            }

            targetOffset = new Vector2(Mathf.Sign(rawOffset.x) * influenceFactorX * MaxMouseOffset, Mathf.Sign(rawOffset.y) * influenceFactorY * MaxMouseOffset);
        }

        m_CurrentMouseOffset = Vector2.SmoothDamp(m_CurrentMouseOffset, targetOffset, ref m_MouseOffsetVelocity, MouseInfluenceSmoothTime);
    }
}