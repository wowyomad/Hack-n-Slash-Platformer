public class EnemyBaseState : IState
{
    public Enemy Self;
    public EnemyBaseState(Enemy self)
    {
        Self = self;
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

    public virtual void Update()
    {
        
    }

    protected void ChangeState(IState state) => Self.StateMachine.ChangeState(state);
}
