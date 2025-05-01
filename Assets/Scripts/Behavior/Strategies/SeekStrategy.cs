using UnityEngine;

namespace Behavior
{
    public class SeekStrategy : IStrategy
    {
        private readonly Enemy Self;
        private readonly Transform m_Transform;
        private readonly Transform TargetTransform;
        private readonly CharacterController2D m_ChracterController;
        private readonly EnemyBehaviorConfigSO Config;
        private readonly NavAgent2D m_NavAgent;

        private bool CanSeePlayer => Self.CanSeePlayer;


        public SeekStrategy(Enemy self, Transform target)   
        {
            Self = self;
            m_Transform = self.transform;
            TargetTransform = target;
            Config = self.BehaviorConfig;
            if (!self.TryGetComponent(out m_ChracterController))
                throw new MissingComponentException("SeekStrategy requires CharacterController2D component");
            if (!self.TryGetComponent(out m_NavAgent))
                throw new MissingComponentException("SeekStrategy requires NavAgent2D component");
        }

        public Node.Status Execute()
        {
            if (CanSeePlayer && Vector2.Distance(m_Transform.position, TargetTransform.position) < Config.VisualSeekDistance)
            {
                return Node.Status.Running;
            }
            return Node.Status.Failure;
        }
    }
}
