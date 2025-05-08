
using TheGame;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class StandartEnemyIdleState : EnemyBaseState, IEnemyVulnarableState
{
    private float m_ChaseDelay = 3.0f;
    private float m_ChaseTimer = 0.0f;
    public StandartEnemyIdleState(Enemy enemy) : base(enemy)
    {

    }

    public override void Enter(IState state)
    {
        m_ChaseTimer = 0.0f;
        Self.Velocity.x = 0.0f;

        Self.OnTakeDamage += OnTakeDamage;
        Self.Hit += OnTakeHit;

        SpriteRenderer sprite;
        if (Self.TryGetComponent(out sprite))
        {
            sprite.color = Color.white;
        }
    }

    public override void Update()
    {
        if (DistanceToPlayer < 10.0f && PlayerIsOnSight)
        {
            m_ChaseTimer += Time.deltaTime;
        }

        if (m_ChaseTimer >= m_ChaseDelay)
        {
            ChangeState(new StandartEnemyChaseState(Self));

        }


    }

    public override void Exit()
    {
        Self.OnTakeDamage -= OnTakeDamage;
        Self.Hit -= OnTakeHit;

    }

    protected void OnTakeDamage(float value, Vector2 direction)
    {
        ChangeState(new StandartEnemyDeadState(Self));
    }
    protected void OnTakeHit()
    {
        ChangeState(new StandartEnemyDeadState(Self));
    }
}