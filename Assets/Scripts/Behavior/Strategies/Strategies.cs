using System;

namespace Behavior
{
    public interface IStrategy
    {
        Node.Status Execute();
        void Reset() { }
    }
    public class  ActionStrategy : IStrategy
    {
        private readonly Action Action;

        public ActionStrategy(Action action)
        {
            Action = action;
        }

        public Node.Status Execute()
        {
            Action.Invoke();
            return Node.Status.Success;
        }
    }

    public class Condition : IStrategy
    {
        private readonly Func<bool> Predicate;

        public Condition(Func<bool> predicate)
        {
            Predicate = predicate;
        }

        public Node.Status Execute() => Predicate.Invoke() ? Node.Status.Success : Node.Status.Failure;
    }
}