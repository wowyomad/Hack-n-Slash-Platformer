using Behavior;
using UnityEngine;

namespace Behavior
{
    public class SeekStrategy : IStrategy
    {
        private Transform m_Transform;
        private Transform TargetTransofrm;
        private CharacterController2D m_ChracterController;
        private float m_Speed;
        private float m_ReachDistance;
        private float m_MaxSeekDistance;
        private Enemy Self;

        public SeekStrategy(GameObject self, Transform targetTransform, float speed, float reachDistance, float maxSeekDistance)
        {
            if (!self.TryGetComponent(out m_ChracterController))
            {
                throw new MissingComponentException("SeekStrategy requires CharacterController2D component");
            }
            m_Transform = self.transform;
            TargetTransofrm = targetTransform;
            m_ReachDistance = reachDistance;
            m_Speed = speed;
            m_MaxSeekDistance = maxSeekDistance;
            Self = self.GetComponent<Enemy>();
        }

        public Node.Status Execute()
        {
            float distance = Vector3.Distance(m_Transform.position, TargetTransofrm.position);
            float horizontalDistance = Mathf.Abs(m_Transform.position.x - TargetTransofrm.position.x);
            if (horizontalDistance >= m_MaxSeekDistance)
            {
                return Node.Status.Failure;
            }

            if (!Self.CanSeePlayer)
            {
                return Node.Status.Failure;
            }
;
            if (distance <= m_ReachDistance)
            {
                return Node.Status.Success;
            }

            Vector3 direction = (TargetTransofrm.position - m_Transform.position).normalized;
            Vector3 horizontalDirection = direction.x > 0 ? Vector3.right : Vector3.left;
            Vector3 displacement = horizontalDirection * m_Speed * Time.deltaTime;

            if (displacement.magnitude > distance)
            {
                displacement = horizontalDirection * distance;
            }


            m_ChracterController.Move(displacement);

            return Node.Status.Running;
        }

    }
}
