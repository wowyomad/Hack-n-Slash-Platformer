using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public class PrioritySelector : Node
    {
        private List<Node> m_SortedChildren;
        public List<Node> SortedChildren => m_SortedChildren ?? Children.OrderByDescending(child => child.Priority).ToList();

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
        public override void Reset()
        {
            base.Reset();
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
        public readonly string Name; //Debugging
        public readonly int Priority;

        protected List<Node> Children;
        public int CurrentChildIndex { get; protected set; }

        public Node(string name = null, int priority = 0, params Node[] children)
        {
            Name = name ?? GetType().Name;
            Priority = priority;
            Children = new List<Node>(children);
        }

        public void AddChild(Node child) => Children.Add(child);
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

    // --- The Builder ---
    public class BehaviorTreeBuilder
    {
        private Node m_RootNode;
        private Stack<Node> m_ParentStack = new Stack<Node>();

        public BehaviorTreeBuilder(string treeName)
        {
            m_RootNode = new BehaviorTree(treeName);
            m_ParentStack.Push(m_RootNode);
        }

        // --- Composite Nodes ---

        public BehaviorTreeBuilder Sequence(string name = null, int priority = 0)
        {
            var sequence = new Sequence(name, priority);
            AddNodeToCurrentParent(sequence);
            m_ParentStack.Push(sequence);
            return this;
        }

        public BehaviorTreeBuilder Selector(string name = null, int priority = 0)
        {
            var selector = new Selector(name, priority);
            AddNodeToCurrentParent(selector);
            m_ParentStack.Push(selector);
            return this;
        }

        public BehaviorTreeBuilder PrioritySelector(string name = null, int priority = 0)
        {
            // Note: The PrioritySelector itself sorts children during Execute.
            // The builder just adds them in the order defined.
            var prioritySelector = new PrioritySelector(name, priority);
            AddNodeToCurrentParent(prioritySelector);
            m_ParentStack.Push(prioritySelector);
            return this;
        }

        // --- Leaf Nodes ---
        public BehaviorTreeBuilder Leaf(Action action)
        {
           return Leaf(null, 0, action);
        }
        public BehaviorTreeBuilder Leaf(string name, Action action)
        {
           return Leaf(name, 0, action);
        }

        public BehaviorTreeBuilder Leaf(string name, int priority, Action action)
        {
            var leaf = new Leaf(name, priority, new ActionStrategy(action));
            AddNodeToCurrentParent(leaf);
            return this;
        }

        public BehaviorTreeBuilder Leaf(string name, IStrategy strategy)
        {
            return Leaf(name, 0, strategy);
        }

        public BehaviorTreeBuilder Leaf(IStrategy strategy)
        {
            return Leaf(null, 0, strategy);
        }

        public BehaviorTreeBuilder Leaf(string name, int priority, IStrategy strategy)
        {
            var leaf = new Leaf(name, priority, strategy);
            AddNodeToCurrentParent(leaf);
            return this;
        }

        /// <summary>
        /// Adds a Leaf node that evaluates a condition (Func<bool>).
        /// </summary>
        public BehaviorTreeBuilder Condition(string name, Func<bool> condition, int priority = 0)
        {
            return Leaf(name, priority, new Condition(condition));
        }

        // --- Structure Management ---

        /// <summary>
        /// Signals the end of defining children for the current composite node (Sequence, Selector, etc.).
        /// Moves the context back up to the parent node.
        /// </summary>
        public BehaviorTreeBuilder End()
        {
            if (m_ParentStack.Count > 1) // Don't pop the root node
            {
                m_ParentStack.Pop();
            }
            else
            {
                // Optional: Log a warning or throw if End() is called too many times
                Debug.LogWarning("BehaviorTreeBuilder: End() called with no parent node on the stack (already at root).");
            }
            return this;
        }

        /// <summary>
        /// Completes the building process and returns the root BehaviorTree node.
        /// </summary>
        public BehaviorTree Build()
        {
            // Ensure we are back at the root level
            while (m_ParentStack.Count > 1)
            {
                End();
            }
            if (m_RootNode is BehaviorTree tree)
            {
                return tree;
            }
            throw new InvalidOperationException("The root node is not a BehaviorTree.");
        }

        // --- Helper ---
        private void AddNodeToCurrentParent(Node node)
        {
            if (m_ParentStack.Count > 0)
            {
                Node parent = m_ParentStack.Peek();
                parent.AddChild(node);
            }
            else
            {
                // This shouldn't happen if initialized correctly with a root
                throw new InvalidOperationException("Cannot add node, no parent node on the stack.");
            }
        }
    }
}
