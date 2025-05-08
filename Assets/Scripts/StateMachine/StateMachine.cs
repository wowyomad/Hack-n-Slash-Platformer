using System;
using System.Collections.Generic;

namespace TheGame
{
    public class CustomStateConfiguration<TState, TTrigger> where TState : class, IState
    {
        private readonly StateMachine<TState, TTrigger> m_OwnerMachine;
        private readonly TState m_SourceState;
        private readonly Stateless.StateMachine<TState, TTrigger>.StateConfiguration m_WrappedConfig;

        public CustomStateConfiguration(
            StateMachine<TState, TTrigger> ownerMachine,
            TState sourceState,
            Stateless.StateMachine<TState, TTrigger>.StateConfiguration underlyingConfig)
        {
            m_OwnerMachine = ownerMachine;
            m_SourceState = sourceState;
            m_WrappedConfig = underlyingConfig;
        }

    
        public CustomStateConfiguration<TState, TTrigger> PermitIf(TTrigger trigger, TState destinationState, Func<bool> guard, string guardDescription = null)
        {
            m_WrappedConfig.PermitIf(trigger, destinationState, guard, guardDescription);
            Action evaluator = () => {
                if (m_OwnerMachine.State == m_SourceState && m_OwnerMachine.CanFire(trigger))
                {
                    m_OwnerMachine.Fire(trigger);
                }
            };
            m_OwnerMachine.AddConditionalEvaluator(evaluator);
            return this;
        }


        public CustomStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState destinationState)
        {
            m_WrappedConfig.Permit(trigger, destinationState);
            return this;
        }


        
        public CustomStateConfiguration<TState, TTrigger> OnEntry(Action entryAction, string description = null)
        {
            m_WrappedConfig.OnEntry(entryAction, description);
            return this;
        }

        public CustomStateConfiguration<TState, TTrigger> OnExit(Action exitAction, string description = null)
        {
            m_WrappedConfig.OnExit(exitAction, description);
            return this;
        }
        
        public CustomStateConfiguration<TState, TTrigger> OnEntryFrom(TTrigger trigger, Action entryAction, string description = null)
        {
            m_WrappedConfig.OnEntryFrom(trigger, entryAction, description);
            return this;
        }
        
        public CustomStateConfiguration<TState, TTrigger> SubstateOf(TState superstate)
        {
            m_WrappedConfig.SubstateOf(superstate);
            return this;
        }

        public CustomStateConfiguration<TState, TTrigger> Ignore(TTrigger trigger)
        {
            m_WrappedConfig.Ignore(trigger);
            return this;
        }
        
        public CustomStateConfiguration<TState, TTrigger> IgnoreIf(TTrigger trigger, Func<bool> guard)
        {
            m_WrappedConfig.IgnoreIf(trigger, guard);
            return this;
        }

        public CustomStateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            m_WrappedConfig.PermitReentry(trigger);
            return this;
        }

        public CustomStateConfiguration<TState, TTrigger> OnActivate(Action action)
        {
            m_WrappedConfig.OnActivate(action);
            return this;
        }
        public CustomStateConfiguration<TState, TTrigger> OnDeactivate(Action action)
        {
            m_WrappedConfig.OnDeactivate(action);
            return this;
        }
    }

    public class StateMachine<TState> : StateMachine<TState, TState> where TState : class, IState
    {
        public StateMachine(TState initialState)
        : base(initialState)
        {
        }
    }

    public class StateMachine<TState, TTrigger> : Stateless.StateMachine<TState, TTrigger> where TState : class, IState
    {
        public event Action<IState, IState> StateChangedEvent;
        public TState PreviousState { get; private set; }

        private readonly List<Action> _conditionalTransitionEvaluators = new List<Action>();

        public StateMachine(TState initialState)
        : base(initialState)
        {
           base.OnTransitioned(OnInternalStateChanged);
        }

        public new CustomStateConfiguration<TState, TTrigger> Configure(TState state)
        {
            var wrappedConfig = base.Configure(state)
                                     .OnEntry(() => state.Enter(base.State))
                                     .OnExit(() => state.Exit());
            return new CustomStateConfiguration<TState, TTrigger>(this, state, wrappedConfig);
        }

        internal void AddConditionalEvaluator(Action evaluator) // Changed to internal
        {
            if (evaluator != null)
            {
                _conditionalTransitionEvaluators.Add(evaluator);
            }
        }

        public void Update()
        {
            if (State != null)
            {
                State.Update();
            }

            var stateBeforeEvaluation = State;
            foreach (var evaluator in _conditionalTransitionEvaluators)
            {
                evaluator.Invoke();
                if (State != stateBeforeEvaluation)
                {
                    break;
                }
            }
        }

        public new void OnTransitioned(Action<Transition> action)
        {
            throw new NotSupportedException("Use StateChangedEvent for external subscriptions. Internal transitions are handled.");
        }

        private void OnInternalStateChanged(Transition transition)
        {
            PreviousState = transition.Source;
            StateChangedEvent?.Invoke(transition.Source, transition.Destination);
        }
    }

    // Step 3: Update StateMachineExtensions
    public static class StateMachineExtensions
    {
        // Extension for CustomStateConfiguration when TState == TTrigger
        public static CustomStateConfiguration<TState, TState> Permit<TState>(
            this CustomStateConfiguration<TState, TState> stateConfiguration,
            TState destinationState) where TState : class, IState
        {
            // Delegate to the wrapper's Permit method that takes two arguments
            return stateConfiguration.Permit(destinationState, destinationState);
        }

        // Extension for CustomStateConfiguration when TState == TTrigger
        public static CustomStateConfiguration<TState, TState> PermitIf<TState>(
            this CustomStateConfiguration<TState, TState> stateConfiguration,
            TState destinationState,
            Func<bool> guard) where TState : class, IState
        {
            // Delegate to the wrapper's PermitIf method that takes two arguments
            // This will then trigger the evaluator registration logic inside the wrapper's PermitIf
            return stateConfiguration.PermitIf(destinationState, destinationState, guard);
        }
    }
}