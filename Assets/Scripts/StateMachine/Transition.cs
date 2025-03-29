public class Transition<TState> : ITransition<TState>
    where TState : class, IState
{
    public TState To { get; private set; }
    public IPredicate Condition { get; private set; }

    public Transition(TState to, IPredicate condition)
    {
        To = to;
        Condition = condition;
    }
}
