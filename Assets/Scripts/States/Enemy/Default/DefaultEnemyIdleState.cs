
using UnityEngine;

public class StandartEnemyIdleState : EnemyBaseState
{
    public StandartEnemyIdleState(Enemy enemy) : base(enemy)
    {

    }

    public override void OnEnter(IState state)
    {
        Self.OnTakeDamage += OnTakeDamage;

        SpriteRenderer sprite;
        if (Self.TryGetComponent(out sprite))
        {
            sprite.color = Color.white;
        }
    }

    public override void OnExit()
    {
        Self.OnTakeDamage -= OnTakeDamage;
    }

    protected void OnTakeDamage(float value, Vector2 direction)
    {
        ChangeState(new StandartEnemyDeadState(Self));
    }
}