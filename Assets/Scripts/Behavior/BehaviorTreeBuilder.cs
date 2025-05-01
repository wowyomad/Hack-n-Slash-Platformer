using System;
using System.Collections.Generic;
using UnityEngine;

namespace Behavior
{
    public class BehaviorTreeBuilder
    {
        private Node m_RootNode;
        private Stack<Node> m_ParentStack = new Stack<Node>();

        public BehaviorTreeBuilder(string treeName)
        {
            m_RootNode = new BehaviorTree(treeName);
            m_ParentStack.Push(m_RootNode);
        }
        public BehaviorTreeBuilder Inverter(int priority = 0)
        {
            return Inverter(null, priority);
        }

        public BehaviorTreeBuilder Inverter(string name, int priority = 0)
        {
            var inverter = new Inverter(name, priority);
            AddNodeToCurrentParent(inverter);
            m_ParentStack.Push(inverter);
            return this;
        }
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
            var prioritySelector = new PrioritySelector(name, priority);
            AddNodeToCurrentParent(prioritySelector);
            m_ParentStack.Push(prioritySelector);
            return this;
        }

        public BehaviorTreeBuilder UntilFail(string name = null, int priority = 0)
        {
            var untilFail = new UntilFail(name, priority);
            AddNodeToCurrentParent(untilFail);
            m_ParentStack.Push(untilFail);
            return this;
        }

        public BehaviorTreeBuilder UntilSuccess(string name = null, int priority = 0)
        {
            var untilSuccess = new UntilSuccess(name, priority);
            AddNodeToCurrentParent(untilSuccess);
            m_ParentStack.Push(untilSuccess);
            return this;
        }

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

        public BehaviorTreeBuilder Leaf(IStrategy strategy)
        {
            return Leaf(null, 0, strategy);
        }

        public BehaviorTreeBuilder Leaf(string name, IStrategy strategy)
        {
            return Leaf(name, 0, strategy);
        }

        public BehaviorTreeBuilder Leaf(string name, int priority, IStrategy strategy)
        {
            var leaf = new Leaf(name, priority, strategy);
            AddNodeToCurrentParent(leaf);
            return this;
        }

        public BehaviorTreeBuilder Do(Action action) => Leaf(action);

        public BehaviorTreeBuilder Do(string name, Action action) => Leaf(name, action);

        public BehaviorTreeBuilder Do(string name, int priority, Action action) => Leaf(name, priority, action);

        public BehaviorTreeBuilder Do(IStrategy strategy) => Leaf(strategy);

        public BehaviorTreeBuilder Do(string name, IStrategy strategy) => Leaf(name, strategy);

        public BehaviorTreeBuilder Do(string name, int priority, IStrategy strategy) => Leaf(name, strategy);


        public BehaviorTreeBuilder Condition(string name, Func<bool> condition, int priority = 0)
        {
            return Leaf(name, priority, new Condition(condition));
        }

        public BehaviorTreeBuilder Condition(string name, int priority, Func<bool> condition)
        {
            return Leaf(name, priority, new Condition(condition));
        }


        public BehaviorTreeBuilder End()
        {
            if (m_ParentStack.Count > 1) 
            {
                m_ParentStack.Pop();
            }
            else
            {
                Debug.LogWarning("End() called with no parent node on the stack");
            }
            return this;
        }

        public BehaviorTree Build()
        {
            if (m_ParentStack.Count > 1)
            {
                throw new InvalidOperationException($"Not all nodes have been closed (Node in stack: {m_ParentStack.Count})");
            }
            if (m_RootNode is BehaviorTree tree)
            {
                End();
                return tree;
            }
            throw new InvalidOperationException("The root node is not a BehaviorTree.");
        }

        private void AddNodeToCurrentParent(Node node)
        {
            if (m_ParentStack.Count > 0)
            {
                Node parent = m_ParentStack.Peek();
                parent.AddChild(node);

                if (parent is SingleChildNode)
                    End();
            }
            else
            {
                throw new InvalidOperationException("Cannot add node, no parent node on the stack.");
            }
        }
    }
}
