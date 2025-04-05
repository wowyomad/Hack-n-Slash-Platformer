using System.Collections.Generic;
using UnityEngine;

namespace Behavior
{
    public class PatrolStrategy : IStrategy
    {
        private readonly Transform m_Transform;
        private readonly List<Transform> m_PatrolPoints;
        private CharacterController2D m_ChracterController;
        private float m_Speed;
        private int m_CurrentPointIndex = 0;

        private static readonly float DistanceBias = 0.025f;

        public PatrolStrategy(GameObject entity, List<Transform> patrolPoints, float speed)
        {
            if(!entity.TryGetComponent(out m_ChracterController))
            {
                throw new MissingComponentException("PatrolStrategy requires CharacterController2D component");
            }

            m_Transform = entity.transform;
            m_PatrolPoints = patrolPoints;
            m_Speed = speed;
        }

        public Node.Status Execute()
        {
            if (m_CurrentPointIndex == m_PatrolPoints.Count)
            {
                m_CurrentPointIndex = 0;
                return Node.Status.Success;
            }

            Vector2 entityPosition = m_Transform.position;
            Vector2 targetPosition = m_PatrolPoints[m_CurrentPointIndex].position;

            targetPosition.y = entityPosition.y;

            if(Vector2.Distance(entityPosition, targetPosition) <= DistanceBias)
            {
                m_CurrentPointIndex++;
            }
            else
            {
                Move(targetPosition);
            }

            return Node.Status.Running;
        }
        public void Reset()
        {
            //m_CurrentPointIndex = 0;
        }

        private void Move(Vector3 destination)
        {
            destination.y = m_Transform.position.y;
            Vector3 direction = (destination - m_Transform.position).normalized;
            Vector3 displacement = direction * m_Speed * Time.deltaTime;

            float distanceToDestination = Vector3.Distance(m_Transform.position, destination);

            if (displacement.magnitude > distanceToDestination)
            {
                displacement = direction * distanceToDestination;
            }

            m_ChracterController.Move(displacement);
        }

    }
}