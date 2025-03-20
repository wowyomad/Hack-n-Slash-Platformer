 using System;

public class FuncPredicate : IPredicate
{
    readonly Func<bool> Predicate;
    public FuncPredicate(Func<bool> predicate) => Predicate = predicate;
    public bool Evaluate() => Predicate.Invoke();
}