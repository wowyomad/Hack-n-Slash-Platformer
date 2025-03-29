public class EnemyBaseState : IEnemyState
{
    public Enemy Self;
    public EnemyBaseState(Enemy self)
    {
        Self = self;
    }

    public virtual void Update()
    {

    }
    public virtual void FixedUpdate()
    {
        
    }

    public virtual void Enter(IState from)
    {
        
    }

    public virtual void Exit()
    {
        
    }


    protected void ChangeState(IEnemyState state) => Self.StateMachine.ChangeState(state);
}
