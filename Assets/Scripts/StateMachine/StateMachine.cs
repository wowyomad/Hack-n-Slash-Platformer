using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StateMachine <TState> where TState : class, IState
{
    public event Action<IState, IState> OnStateChange;

    private StateNode m_Current;
    private Dictionary<Type, StateNode> m_Nodes = new();
    private HashSet<Transition<TState>> m_AnyTransitions = new();

    private TState m_PendingState = null;
    private StateNode m_EmptyStateNode;
    public TState CurrentState
    {
        get
        {
            return m_Current?.State;
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
            var transition = GetTransition();
            if (transition != null)
            {
                SwitchState(transition.To);
            }
        }

        CurrentState?.Update();
    }

    public void AddAnyTransition(TState to, Func<bool> condition)
    {
        var predicate = new FuncPredicate(condition);
        AddAnyTransition(to, predicate);
    }

    public void AddAnyTransition(TState to, IPredicate condition)
    {
        if (!m_Nodes.ContainsKey(to.GetType()))
            m_Nodes.Add(to.GetType(), new StateNode(to));
        m_AnyTransitions.Add(new Transition<TState>(to, condition));
    }

    public void AddTransition(TState from, TState to, Func<bool> condition)
    {
        var predicate = new FuncPredicate(condition);
        AddTransition(from, to, predicate);
    }

    public void AddTransition(TState from, TState to, IPredicate condition)
    {
        if (!m_Nodes.ContainsKey(from.GetType()))
            m_Nodes.Add(from.GetType(), new StateNode(from));
        if (!m_Nodes.ContainsKey(to.GetType()))
            m_Nodes.Add(to.GetType(), new StateNode(to));

        m_Nodes[from.GetType()].AddTransition(to, condition);
    }

    public void ChangeState(TState state)
    {
        if (CurrentState?.GetType() == state.GetType())
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

    public void SwitchState(TState state)
    {
        if (CurrentState == state || !m_Nodes.ContainsKey(state.GetType()))
            return;

        PerformStateTransition(state);
    }

    private void ChangeCurrentStateToPending()
    {
        if (m_PendingState != null)
        {
            TState newState = m_PendingState;
            m_PendingState = null;
            PerformStateTransition(newState);
        }
    }

    private StateNode GetOrCreateStateNode(TState state)
    {
        return new StateNode(state);
    }

    private Transition<TState> GetTransition()
    {
        if (m_Current == null)
        {
            Debug.LogWarning("Current state is null");
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

    private void PerformStateTransition(TState newState)
    {
        IState previousState = m_Current?.State;

        previousState?.Exit();
        m_Current = GetOrCreateStateNode(newState);
        m_Current.State.Enter(previousState);

        OnStateChange?.Invoke(previousState, newState);
    }

    [System.Serializable]
    private class StateNode
    {
        [SerializeField] public TState State { get; private set; }
        public HashSet<Transition<TState>> Transitions { get; }

        public StateNode(TState state)
        {
            State = state;
            Transitions = new HashSet<Transition<TState>>();
        }
        public void AddTransition(TState to, IPredicate condition)
        {
            Debug.Log($"Added transition from {State.GetType()} to {to.GetType()}");
            Transitions.Add(new Transition<TState>(to, condition));
        }
    }
}
