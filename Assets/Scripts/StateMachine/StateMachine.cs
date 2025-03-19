using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StateMachine
{
    [SerializeField] StateNode m_Current;
    [SerializeField] Dictionary<Type, StateNode> m_Nodes = new();
    [SerializeField] HashSet<ITransition> m_AnyTransitions = new();

    public void Update()
    {
        ITransition transition = GetTransition();
        if (transition != null)
        {
            Debug.Log($"State transition from {m_Current.State.GetType()} to {transition.To.GetType()}");
            SwitchState(transition.To);
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
        if(!m_Nodes.ContainsKey(to.GetType()))
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

    public void SetState(IState state)
    {
        if (m_Current?.State == state || !m_Nodes.ContainsKey(state.GetType()))
            return;

        IState previous = m_Current.State;

        previous?.OnExit();
        m_Current = m_Nodes[state.GetType()];
        m_Current.State.OnEnter(previous);
    }

    private void SwitchState(IState state)
    {
        if (m_Current?.State == state || !m_Nodes.ContainsKey(state.GetType()))
            return;

        IState previous = m_Current.State;

        previous?.OnExit();
        m_Current = m_Nodes[state.GetType()];
        m_Current.State.OnEnter(previous);
    }

    private ITransition GetTransition()
    {
        if(m_Current == null)
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
        [SerializeField] public IState State { get; }
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
