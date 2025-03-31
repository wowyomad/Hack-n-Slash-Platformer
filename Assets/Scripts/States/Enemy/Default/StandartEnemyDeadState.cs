using UnityEngine;

public class StandartEnemyDeadState : EnemyBaseState
{
    public float TimeToDestroy = 3.0f;
    float m_Timer = 0f;
    public StandartEnemyDeadState(Enemy self) : base(self) { }

    public override void Enter(IState from)
    {
        Self.Velocity.x = 0.0f;
        SpriteRenderer sprite;
        if (Self.TryGetComponent(out sprite))
        {
            sprite.color = Color.red;
        }
        m_Timer = 0f;
    }

    public override void Update()
    {
        if (m_Timer >= TimeToDestroy)
        {
            GameObject.Destroy(Self.gameObject);
        }
        m_Timer += Time.deltaTime;
    }

}