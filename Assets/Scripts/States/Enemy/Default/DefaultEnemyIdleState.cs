
using UnityEngine;

public class StandartEnemyIdleState : EnemyBaseState, IEnemyVulnarableState
{
    public StandartEnemyIdleState(Enemy enemy) : base(enemy)
    {

    }

    public override void Enter(IState state)
    {
        Self.OnTakeDamage += OnTakeDamage;
        Self.OnHit += OnTakeHit;

        SpriteRenderer sprite;
        if (Self.TryGetComponent(out sprite))
        {
            sprite.color = Color.white;
        }
    }

    public override void Exit()
    {
        Self.OnTakeDamage -= OnTakeDamage;
        Self.OnHit -= OnTakeHit;

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