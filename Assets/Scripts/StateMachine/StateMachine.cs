using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    StateNode m_Current;
    Dictionary<Type, StateNode> m_Nodes = new();
    HashSet<ITransition> m_AnyTransitions = new();

    public void Update()
    {
        ITransition transition = GetTransition();
        if (transition != null)
        {
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

        m_Current = m_Nodes[state.GetType()];
        m_Current.State.OnEnter();
    }

    private void SwitchState(IState state)
    {
        if (m_Current?.State == state || !m_Nodes.ContainsKey(state.GetType()))
            return;
          
        m_Current.State.OnExit();
        m_Current = m_Nodes[state.GetType()];
        m_Current.State.OnEnter();  
    }

    private ITransition GetTransition()
    {
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

    class StateNode
    {
        public IState State { get; }
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
