using System;
using System.Collections.Generic;
using System.Linq;

namespace Behavior
{
    public class BehaviorTree : Node
    {
        public BehaviorTree() { }
        public BehaviorTree(string name) : base(name) { }

        public override Status Execute()
        {
            CurrentChildIndex = 0;
            foreach (Node child in Children)
            {
                Status status = child.Execute();
                if (status != Status.Success)
                {
                    return status;
                }
                CurrentChildIndex++;
            }

            return Status.Success;
        }
    }

    public class UntilSuccess : SingleChildNode
    {
        public UntilSuccess(Node child) : this(null, child) { }
        public UntilSuccess(string name, Node child) : this(name, 0, child) { }
        public UntilSuccess(string name, int priority, Node child) : base(name, priority, child) { }
        public UntilSuccess(string name, int priority) : base(name, priority) { }


        public override Status Execute()
        {
            var status = Children[0].Execute();
            if (status == Status.Success)
            {
                Reset();
                return Status.Success;
            }

            return Status.Running;
        }
    }

    public class UntilFail : SingleChildNode
    {
        public UntilFail(Node child) : this(null, child) { }
        public UntilFail(string name, Node child) : base(name, 0, child) { }
        public UntilFail(string name, int priority, Node child) : base(name, priority, child) { }
        public UntilFail(string name, int priority) : base(name, priority) { }
        public override Status Execute()
        {
            var status = Children[0].Execute();
            if (status == Status.Failure)
            {
                Reset();
                return Status.Success;
            }

            return Status.Running;
        }
    }
    

    public class Inverter : SingleChildNode
    {
        public Inverter(Node child) : this(null, child) { }
        public Inverter(string name, Node child) : this(name, 0, child) { }
        public Inverter(string name, int priority, Node child) : base(name, priority, child) { }
        public Inverter(string name, int priority) : base(name, priority) { }

        public override Status Execute()
        {
            var status = Children[0].Execute();
            return status switch
            {
                Status.Running => Status.Running,
                Status.Success => Status.Failure,
                Status.Failure => Status.Success,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public class Selector : Node
    {
        public Selector(params Node[] children) : this(null, children) { }
        public Selector(string name, params Node[] children) : this(name, 0, children) { }
        public Selector(string name, int priority, params Node[] children) : base(name, priority, children) { }

        public override Status Execute()
        {
            if (CurrentChildIndex < Children.Count)
            {
                Status status = Children[CurrentChildIndex].Execute();
                switch (status)
                {
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        CurrentChildIndex++;
                        return Status.Running;
                }
            }

            Reset();
            return Status.Failure;
        }
    }

    public class PrioritySelector : Selector
    {
        private List<Node> m_SortedChildren;
        public virtual List<Node> SortedChildren => m_SortedChildren ?? Children.OrderByDescending(child => child.Priority).ToList();

        public PrioritySelector(params Node[] children) : this(null, children) { }
        public PrioritySelector(string name, params Node[] children) : this(name, 0, children) { }

        public PrioritySelector(string name, int priority, params Node[] children) : base(name, priority, children) { }

        public override Status Execute()
        {
            foreach (var child in SortedChildren)
            {
                var status = child.Execute();
                switch (status)
                {
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        continue;
                }
            }

            Reset();
            return Status.Failure;
        }
        public override void AddChild(Node child)
        {
            base.AddChild(child);
            m_SortedChildren = null;
        }

        public override void Reset()
        {
            base.Reset();
        }
    }

    public class RandomSelector : PrioritySelector
    {
        private Random m_Rng = new Random();
        public override List<Node> SortedChildren => Children.OrderBy(_ => m_Rng.Next()).ToList();

        public RandomSelector(params Node[] children) : this(null, children) { }
        public RandomSelector(string name, params Node[] children) : this(name, 0, children) { }

        public RandomSelector(string name, int priority, params Node[] children) : base(name, priority, children) { }
    }



    public class Sequence : Node
    {
        public Sequence(params Node[] children) : this(null, children) { }

        public Sequence(string name, params Node[] children) : this(name, 0, children) { }

        public Sequence(string name, int priority, params Node[] children) : base(name, priority, children) { }

        public override Status Execute()
        {
            if (CurrentChildIndex < Children.Count)
            {
                Status status = Children[CurrentChildIndex].Execute();
                switch (status)
                {
                    case Status.Success:
                        CurrentChildIndex++;
                        return CurrentChildIndex == Children.Count ? Status.Success : Status.Running;
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        Reset();
                        return Status.Failure;
                }
            }

            Reset();
            return Status.Success;
        }
    }

    public class SingleChildNode : Node
    {
        public SingleChildNode(Node child) : this(null, child) { }
        public SingleChildNode(string name, Node child) : this(name, 0, child) { }
        public SingleChildNode(string name, int priority, Node child) : base(name, priority, child) { }
        public SingleChildNode(string name, int priority) : base(name, priority) { }

        public override Status Execute() => Children[0].Execute();
        public override void AddChild(Node child)
        {
            if (Children.Count > 0)
            {
                throw new InvalidOperationException($"{Name} can only have one child.");
            }
            base.AddChild(child);
        }
    }

    public class Leaf : Node
    {
        private readonly IStrategy m_Strategy;

        public Leaf(IStrategy strategy) : this(null, strategy) { }

        public Leaf(string name, IStrategy strategy) : this(name, 0, strategy) { }

        public Leaf(string name, int priority, IStrategy strategy) : base(name, priority)
        {
            m_Strategy = strategy;
        }

        public override Status Execute() => m_Strategy.Execute();
        public override void Reset() => m_Strategy.Reset();
    }

    public abstract class Node
    {
        public readonly string Name;
        public readonly int Priority;

        protected List<Node> Children;
        public int CurrentChildIndex { get; protected set; }

        public Node(string name = null, int priority = 0, params Node[] children)
        {
            Name = name ?? GetType().Name;
            Priority = priority;
            if (children.All(child => child != null))
                Children = new List<Node>(children);
            else
                Children = new List<Node>();
        }

        public virtual void AddChild(Node child) => Children.Add(child);
        public virtual Status Execute() => Children[CurrentChildIndex].Execute();

        public virtual void Reset()
        {
            CurrentChildIndex = 0;
            foreach (Node child in Children)
            {
                child.Reset();
            }
        }

        public enum Status
        {
            Success,
            Failure,
            Running
        }
    }
}
