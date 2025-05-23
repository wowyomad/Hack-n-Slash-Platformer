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

            [Header("Debug")]
            [SerializeField] private bool m_DrawGizmos = true;
            [SerializeField] private bool m_DrawGizmosOnlyWhenSelected = true;

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
                if (m_DrawGizmos && m_DrawGizmosOnlyWhenSelected)
                {
                    DrawGizmos();
                }
            }

            private void OnDrawGizmos()
            {
                if (m_DrawGizmos && !m_DrawGizmosOnlyWhenSelected)
                {
                    DrawGizmos();
                }
            }

            private void DrawGizmos()
            {
                int direction = Owner != null ? Owner.FacingDirection : 1;

                if (Owner == null)
                {
                    DrawVisionCone(m_EyesightDistance * m_AlertedDistanceMultiplier, m_EyesightAngle, Color.yellow, direction);
                    DrawVisionCone(m_BacksightDistance * m_AlertedDistanceMultiplier, m_BacksightAngle, Color.yellow, -direction);
                    DrawVisionCone(m_EyesightDistance, m_EyesightAngle, Color.green, direction);
                    DrawVisionCone(m_BacksightDistance, m_BacksightAngle, Color.green, -direction);

                    DrawVisionCircle(m_CloseSightRadius, Color.green);
                    DrawVisionCircle(m_AlertedCloseSightRadius, Color.yellow);
                }
                else if (Owner.IsAlerted)
                {
                    DrawVisionCone(m_EyesightDistance * m_AlertedDistanceMultiplier, m_EyesightAngle, Color.yellow, direction);
                    DrawVisionCone(m_BacksightDistance * m_AlertedDistanceMultiplier, m_BacksightAngle, Color.yellow, -direction);
                    DrawVisionCircle(m_AlertedCloseSightRadius, Color.yellow);

                }
                else
                {
                    DrawVisionCone(m_EyesightDistance, m_EyesightAngle, Color.green, direction);
                    DrawVisionCone(m_BacksightDistance, m_BacksightAngle, Color.green, -direction);
                    DrawVisionCircle(m_CloseSightRadius, Color.green);

                }
            }

            private void DrawVisionCircle(float radius, Color color)
            {
                GizmosEx.DrawCircle(transform.position, radius, color);
            }

            private void DrawVisionCone(float distance, float angle, Color color, int direction)
            {
                Vector2 dir = new Vector2(direction, 0.0f);
                GizmosEx.DrawCone(transform.position, dir, distance, angle, color);
            }
#endif
        }
    }
}