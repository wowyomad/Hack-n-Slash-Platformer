using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StateMachine
{
    StateNode m_Current;
    Dictionary<Type, StateNode> m_Nodes = new();
    HashSet<ITransition> m_AnyTransitions = new();

    private IState m_PendingState = null;
    private StateNode m_EmptyStateNode;

    public IState Current
    {
        get
        {
            return m_Current?   .State;
        }
    }

    public void Update()
    {
        if (m_PendingState != null)
        {
            ChangeCurrentStateToPending();
        }
        else 
        {
            ITransition transition = GetTransition();
            if (transition != null)
            {
                SwitchState(transition.To);
            }
        }

        m_Current.State.Update();
    }

    public void AddAnyTransition(IState to, Func<bool> condition)
    {
        var predicate = new FuncPredicate(condition);
        AddAnyTransition(to, predicate);
    }

    public void AddAnyTransition(IState to, IPredicate condition)
    {
        if (!m_Nodes.ContainsKey(to.GetType()))
            m_Nodes.Add(to.GetType(), new StateNode(to));
        m_AnyTransitions.Add(new Transition(to, condition));
    }

    public void AddTransition(IState from, IState to, Func<bool> condition)
    {
        var predicate = new FuncPredicate(condition);
        AddTransition(from, to, predicate);
    }

    public void AddTransition(IState from, IState to, IPredicate condition)
    {
        if (!m_Nodes.ContainsKey(from.GetType()))
            m_Nodes.Add(from.GetType(), new StateNode(from));
        if (!m_Nodes.ContainsKey(to.GetType()))
            m_Nodes.Add(to.GetType(), new StateNode(to));

        m_Nodes[from.GetType()].AddTransition(to, condition);
    }

    public void ChangeState(IState state)
    {
        if (m_Current?.State?.GetType() == state.GetType())
        {
            Debug.LogWarning($"State {state} is already active");
            return;
        }
        if (m_PendingState != null)
        {
            Debug.LogWarning($"State Machine already has a pending state: {state}");
            return;
        }
        m_PendingState = state;
    }

    private void ChangeCurrentStateToPending()
    {
        if (m_PendingState != null)
        {
            IState newState = m_PendingState;
            m_PendingState = null;

            IState previousState = m_Current?.State;

            if (previousState != null)
            {
                Debug.Log($"Change from {previousState.GetType()} to {newState.GetType()}");
            }
            else
            {
                Debug.Log($"Change to {newState.GetType()}");
            }

            previousState?.Exit();
            m_Current = GetOrCreateStateNode(newState);
            m_Current.State.Enter(previousState);
        }
    }

    public void SwitchState(IState state)
    {
        if (m_Current?.State == state || !m_Nodes.ContainsKey(state.GetType()))
            return;

        IState previous = m_Current.State;
        if (previous != null)
        {
            Debug.Log($"Transition from {previous.GetType()} to {state}");
        }
        else
        {
            Debug.Log($"Transition to {state}");
        }

        previous?.Exit();
        m_Current = m_Nodes[state.GetType()];
        m_Current.State.Enter(previous);
    }

    private StateNode GetOrCreateStateNode(IState state)
    {
       return new StateNode(state);
    }

    private ITransition GetTransition()
    {
        if (m_Current == null)
        {
            Debug.Log("Current state is null");
            return null;
        }
        foreach (var transition in m_AnyTransitions)
        {
            if (transition.Condition.Evaluate())
                return transition;
        }

        foreach (var transition in m_Current.Transitions)
        {
            if (transition.Condition.Evaluate())
                return transition;
        }

        return null;
    }

    [System.Serializable]
    class StateNode
    {
        [SerializeField] public IState State { get; private set; }
        public HashSet<ITransition> Transitions { get; }

        public StateNode(IState state)
        {
            State = state;
            Transitions = new HashSet<ITransition>();
        }
        public void AddTransition(IState to, IPredicate condition)
        {
            Debug.Log($"Added transition from {State.GetType()} to {to.GetType()}");
            Transitions.Add(new Transition(to, condition));
        }
    }
}
