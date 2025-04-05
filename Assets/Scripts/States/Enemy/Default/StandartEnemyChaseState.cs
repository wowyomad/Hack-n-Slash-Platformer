using Unity.VisualScripting;
using UnityEngine;

public class StandartEnemyChaseState : EnemyBaseState, IEnemyVulnarableState
{
    private float m_TurnDelay = 0.5f;
    private float m_TurnTimer = 0.0f;
    private int m_PreviousPlayerDirection = 0;
    public StandartEnemyChaseState(Enemy enemy) : base(enemy)
    {

    }
    public override void Enter(IState state)
    {
        Self.Hit += OnTakeHit;
        m_PreviousPlayerDirection = DirectionToPlayer;
        Self.GetComponent<SpriteRenderer>().color = Color.red;
    }
    public override void Update()
    {
        Self.ApplyGravityToVelocity();

        if (DistanceToPlayer > 10.0f || !PlayerIsOnSight)
        {
            ChangeState(new StandartEnemyIdleState(Self));
        }

        if (DistanceToPlayer > 0.25f)
        {
            int direction = DirectionToPlayer;
            if (m_PreviousPlayerDirection != direction)
            {
                m_TurnTimer += Time.deltaTime;
                if (m_TurnTimer >= m_TurnDelay)
                {
                    m_PreviousPlayerDirection = direction;
                    m_TurnTimer = 0.0f;
                }
            }
            Self.Velocity.x = m_PreviousPlayerDirection * Self.Movement.HorizontalSpeed;
        }
        else
        {
            Self.Velocity.x = 0;
        }


        if (DistanceToPlayer < 0.25f)
        {
           
            IHittable hittable;
            if (Self.PlayerReference.TryGetComponent(out hittable) && hittable.CanTakeHit)
            {
                hittable.TakeHit();
                ChangeState(new StandartEnemyIdleState(Self));
            }
        }


    }
    public override void Exit()
    {
        Self.Hit -= OnTakeHit;
    }
    protected void OnTakeHit()
    {
        ChangeState(new StandartEnemyDeadState(Self));
    }
}