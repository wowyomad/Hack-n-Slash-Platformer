using TheGame;

public interface ITransition <TState> where TState : class, IState
{
    TState To { get; }
    IPredicate Condition { get; }
}
