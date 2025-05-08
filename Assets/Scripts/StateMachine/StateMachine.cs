using System;
using System.Collections.Generic;

namespace TheGame
{
    public class StateConfigurationWrapper<TState, TTrigger> where TState : class, IState
    {
        private readonly StateMachine<TState, TTrigger> m_OwnerMachine;
        private readonly TState m_SourceState;
        private readonly Stateless.StateMachine<TState, TTrigger>.StateConfiguration m_WrappedConfig;

        public StateConfigurationWrapper(
            StateMachine<TState, TTrigger> ownerMachine,
            TState sourceState,
            Stateless.StateMachine<TState, TTrigger>.StateConfiguration underlyingConfig)
        {
            m_OwnerMachine = ownerMachine;
            m_SourceState = sourceState;
            m_WrappedConfig = underlyingConfig;
        }

        public StateConfigurationWrapper<TState, TTrigger> PermitIf(TTrigger trigger, TState destinationState, Func<bool> guard, string guardDescription = null)
        {
            m_WrappedConfig.PermitIf(trigger, destinationState, guard, guardDescription);

            Action conditionalCheck = () =>
            {
                if (m_OwnerMachine.CanFire(trigger))
                {
                    m_OwnerMachine.Fire(trigger);
                }
            };
            m_OwnerMachine.AddStateTrigger(m_SourceState, conditionalCheck);
            return this;
        }

        public StateConfigurationWrapper<TState, TTrigger> Permit(TTrigger trigger, TState destinationState)
        {
            m_WrappedConfig.Permit(trigger, destinationState);
            return this;
        }

        public StateConfigurationWrapper<TState, TTrigger> OnEntry(Action entryAction, string description = null)
        {
            m_WrappedConfig.OnEntry(entryAction, description);
            return this;
        }

        public StateConfigurationWrapper<TState, TTrigger> OnExit(Action exitAction, string description = null)
        {
            m_WrappedConfig.OnExit(exitAction, description);
            return this;
        }

        public StateConfigurationWrapper<TState, TTrigger> OnEntryFrom(TTrigger trigger, Action entryAction, string description = null)
        {
            m_WrappedConfig.OnEntryFrom(trigger, entryAction, description);
            return this;
        }

        public StateConfigurationWrapper<TState, TTrigger> SubstateOf(TState superstate)
        {
            m_WrappedConfig.SubstateOf(superstate);
            return this;
        }

        public StateConfigurationWrapper<TState, TTrigger> Ignore(TTrigger trigger)
        {
            m_WrappedConfig.Ignore(trigger);
            return this;
        }

        public StateConfigurationWrapper<TState, TTrigger> IgnoreIf(TTrigger trigger, Func<bool> guard)
        {
            m_WrappedConfig.IgnoreIf(trigger, guard);
            return this;
        }

        public StateConfigurationWrapper<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            m_WrappedConfig.PermitReentry(trigger);
            return this;
        }

        public StateConfigurationWrapper<TState, TTrigger> OnActivate(Action action)
        {
            m_WrappedConfig.OnActivate(action);
            return this;
        }
        public StateConfigurationWrapper<TState, TTrigger> OnDeactivate(Action action)
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

        private readonly Dictionary<TState, List<Action>> m_StatesTransitions =
            new Dictionary<TState, List<Action>>();

        public StateMachine(TState initialState)
        : base(initialState)
        {
            base.OnTransitioned(OnInternalStateChanged);
        }

        public new StateConfigurationWrapper<TState, TTrigger> Configure(TState state)
        {
            var statelessConfig = base.Configure(state)
                                     .OnEntry(() => state.OnEnter())
                                     .OnExit(() => state.OnExit());
            return new StateConfigurationWrapper<TState, TTrigger>(this, state, statelessConfig);
        }

        internal void AddStateTrigger(TState sourceState, Action triggerEvaluator)
        {
            if (triggerEvaluator == null || sourceState == null) return;

            if (!m_StatesTransitions.TryGetValue(sourceState, out var evaluator))
            {
                evaluator = new List<Action>();
                m_StatesTransitions[sourceState] = evaluator;
            }
            evaluator.Add(triggerEvaluator);
        }

        public void Update()
        {
            if (State == null)
            {
                return; 
            }

            EvaluateState();
            State?.OnUpdate();
        }

        public new void OnTransitioned(Action<Stateless.StateMachine<TState, TTrigger>.Transition> action)
        {
            throw new NotSupportedException("Use StateChangedEvent for external subscriptions. Internal transitions are handled.");
        }

        private void OnInternalStateChanged(Stateless.StateMachine<TState, TTrigger>.Transition transition)
        {
            PreviousState = transition.Source;
            StateChangedEvent?.Invoke(transition.Source, transition.Destination);
        }


        private void EvaluateState()
        {
            bool stateChanged;
            do
            {
                stateChanged = false;
                TState initialState = State;

                if (m_StatesTransitions.TryGetValue(initialState, out var triggerEvaluators))
                {
                    foreach (var evaluator in triggerEvaluators)
                    {
                        evaluator.Invoke();

                        if (State != initialState)
                        {
                            stateChanged = true;
                            break;
                        }
                    }
                }
            } while (stateChanged && State != null);
        }
    }


    public static class StateMachineExtensions
    {
        public static StateConfigurationWrapper<TState, TState> Permit<TState>(
            this StateConfigurationWrapper<TState, TState> stateConfiguration,
            TState destinationState) where TState : class, IState
        {
            return stateConfiguration.Permit(destinationState, destinationState);
        }

        public static StateConfigurationWrapper<TState, TState> PermitIf<TState>(
            this StateConfigurationWrapper<TState, TState> stateConfiguration,
            TState destinationState,
            Func<bool> guard) where TState : class, IState
        {
            return stateConfiguration.PermitIf(destinationState, destinationState, guard);
        }
    }
}