using UnityEngine;

namespace TheGame
{
    namespace EnemySensor
    {
        
        public class VisualSensor : Sensor
        {
            [Header("Visual Parameters")]
            [SerializeField] private float m_EyesightDistance = 12.0f;
            [SerializeField] private float m_EyesightAngle = 60.0f;
            [SerializeField] private float m_BacksightDistance = 5.0f;
            [SerializeField] private float m_BacksightAngle = 45.0f;
            [SerializeField] private float m_CloseSightRadius = 3.0f;
            [SerializeField] private float m_AlertedCloseSightRadius = 10.0f;

            [Header("Alerted State Modifiers")]
            [SerializeField] private float m_AlertedDistanceMultiplier = 1.4f;

            [Header("Occlusion Layers")]
            [SerializeField] private LayerMask m_StandardOcclusionMask;
            [SerializeField] private LayerMask m_AlertedOcclusionMask;

            private void Awake()
            {
                Initialize(GetComponent<Enemy>());
            }

            public override bool Check(GameObject target)
            {
                if (Owner == null || target == null) return false;

                bool isAlerted = Owner.IsAlerted;
                int facingDirection = Owner.FacingDirection;
                Vector3 sensorPosition = transform.position;

                Vector3 targetPosition = target.transform.position;
                float distanceToTarget = Vector3.Distance(sensorPosition, targetPosition);

                float currentCloseRadius = isAlerted ? m_AlertedCloseSightRadius : m_CloseSightRadius;
                LayerMask currentLayerMask = isAlerted ? m_AlertedOcclusionMask : m_StandardOcclusionMask;

                // 1. Close Circle Sight
                if (distanceToTarget <= currentCloseRadius)
                {
                    if (!IsOccluded(sensorPosition, targetPosition, distanceToTarget, currentLayerMask, target))
                    {
                        return true;
                    }
                }

                // 2. Far Cone Sight
                Vector3 forwardDir = new Vector3(facingDirection, 0.0f, 0.0f);
                Vector3 backwardDir = -forwardDir;

                // Front
                float currentEyesightDist = m_EyesightDistance * (isAlerted ? m_AlertedDistanceMultiplier : 1f);
                if (IsInCone(targetPosition, sensorPosition, forwardDir, m_EyesightAngle, currentEyesightDist))
                {
                    if (!IsOccluded(sensorPosition, targetPosition, distanceToTarget, currentLayerMask, target))
                    {
                        return true;
                    }
                }

                // Back
                float currentBacksightDist = m_BacksightDistance * (isAlerted ? m_AlertedDistanceMultiplier : 1f);
                if (IsInCone(targetPosition, sensorPosition, backwardDir, m_BacksightAngle, currentBacksightDist))
                {
                    if (!IsOccluded(sensorPosition, targetPosition, distanceToTarget, currentLayerMask, target))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool IsInCone(Vector3 targetPos, Vector3 coneOrigin, Vector3 coneWorldDirection, float coneAngleDegrees, float coneMaxDistance)
            {
                Vector3 dirToTarget = targetPos - coneOrigin;
                float distToTargetSqr = dirToTarget.sqrMagnitude;

                if (distToTargetSqr == 0f)
                {
                    return true;
                }

                if (distToTargetSqr > coneMaxDistance * coneMaxDistance) return false;

                float angleToTarget = Vector3.Angle(coneWorldDirection, dirToTarget.normalized);
                return angleToTarget <= coneAngleDegrees / 2f;
            }

            private bool IsOccluded(Vector3 origin, Vector3 targetPositionToCheck, float distanceToTarget, LayerMask mask, GameObject targetObject)
            {
                if (distanceToTarget < 0.01f) return true;

                Vector3 direction = (targetPositionToCheck - origin).normalized;
                if (direction == Vector3.zero) return true;

                RaycastHit2D hit = Physics2D.Raycast(origin, direction, distanceToTarget, mask);
                return hit.collider != null;
            }


#if UNITY_EDITOR
            private void OnDrawGizmosSelected()
            {
                DrawVisionCone(m_EyesightDistance * m_AlertedDistanceMultiplier, m_EyesightAngle, Color.yellow * 0.5f);
                DrawVisionCone(m_BacksightDistance * m_AlertedDistanceMultiplier, m_BacksightAngle, Color.yellow * 0.5f, true);
                DrawVisionCone(m_EyesightDistance, m_EyesightAngle, Color.green);
                DrawVisionCone(m_BacksightDistance, m_BacksightAngle, Color.green, true);


                DrawVisionCircle(m_CloseSightRadius, Color.blue);
                DrawVisionCircle(m_AlertedCloseSightRadius, Color.red);
            }

            private void DrawVisionCircle(float radius, Color color)
            {
                Gizmos.color = color;
                Gizmos.DrawWireSphere(transform.position, radius);
            }

            private void DrawVisionCone(float distance, float angle, Color color, bool isBacksight = false)
            {
                Gizmos.color = color;
                Vector3 forwardDirection = transform.right * (isBacksight ? -1 : 1);

                Quaternion upRayRotation = Quaternion.AngleAxis(-angle / 2, Vector3.forward);
                Quaternion downRayRotation = Quaternion.AngleAxis(angle / 2, Vector3.forward);

                Vector3 upRayDirection = upRayRotation * forwardDirection;
                Vector3 downRayDirection = downRayRotation * forwardDirection;

                Gizmos.DrawRay(transform.position, upRayDirection * distance);
                Gizmos.DrawRay(transform.position, downRayDirection * distance);


                int segments = 20;
                float segmentAngle = angle / segments;
                Vector3 prevPoint = transform.position + upRayDirection * distance;

                for (int i = 1; i <= segments; i++)
                {
                    Quaternion segmentRotation = Quaternion.AngleAxis(segmentAngle * i - angle / 2, Vector3.forward);
                    Vector3 currentPointDirection = segmentRotation * forwardDirection;
                    Vector3 currentPoint = transform.position + currentPointDirection * distance;
                    Gizmos.DrawLine(prevPoint, currentPoint);
                    prevPoint = currentPoint;
                }
            }
#endif
        }
    }
}