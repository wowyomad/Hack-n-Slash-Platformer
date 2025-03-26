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

    public virtual void OnEnter(IState from)
    {
        
    }

    public virtual void OnExit()
    {
        
    }

    public virtual void Update()
    {
        
    }

    protected void ChangeState(IState state) => Self.StateMachine.ChangeState(state);
}
