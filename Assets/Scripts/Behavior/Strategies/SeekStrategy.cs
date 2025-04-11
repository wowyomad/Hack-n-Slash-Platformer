using Behavior;
using UnityEngine;

namespace Behavior
{
    public class SeekStrategy : IStrategy
    {
        private Transform m_Transform;
        private Transform TargetTransofrm;
        private CharacterController2D m_ChracterController;
        private EnemyBehaviorConfigSO Config;
        private Enemy Self;

        private bool CanSeePlayer => Self.CanSeePlayer;

        public SeekStrategy(Enemy self, Transform target)
        {
            if (!self.TryGetComponent(out m_ChracterController))
            {
                throw new MissingComponentException("SeekStrategy requires CharacterController2D component");
            }
            m_Transform = self.transform;
            TargetTransofrm = target;
            Self = self.GetComponent<Enemy>();
            Config = self.BehaviorConfig;
        }

        public Node.Status Execute()
        {
            float distance = Vector3.Distance(m_Transform.position, TargetTransofrm.position);
            float horizontalDistance = Mathf.Abs(m_Transform.position.x - TargetTransofrm.position.x);
            if (horizontalDistance >= Config.VisualSeekDistance)
            {
                return Node.Status.Failure;
            }

            if (!CanSeePlayer)
            {
                return Node.Status.Failure;
            }
;
            if (distance <= Config.SeekStoppingDistance)
            {
                return Node.Status.Success;
            }

            Vector3 direction = (TargetTransofrm.position - m_Transform.position).normalized;
            Vector3 horizontalDirection = direction.x > 0 ? Vector3.right : Vector3.left;
            Vector3 displacement = horizontalDirection * Config.SeekSpeed * Time.deltaTime;

            if (displacement.magnitude > distance)
            {
                displacement = horizontalDirection * distance;
            }


            m_ChracterController.Move(displacement);

            return Node.Status.Running;
        }

    }
}
