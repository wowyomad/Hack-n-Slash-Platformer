using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheGame
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Lever : MonoBehaviour, IHittable
    {
        public UnityEvent OnActivate;
        public UnityEvent OnDeactivate;
        public event Action OnHit;

        [SerializeField] private float m_ActivationDelay = 0.5f;
        [SerializeField] private LayerMask m_WhoAllowedToHit;
        [SerializeField] protected string AnimationTriggerActivate = "Activate";

        private Animator m_Animator;
        private StateMachine<LeverState, Trigger> StateMachine;
        private LeverState.Activated ActivatedState;
        private LeverState.Deactivated DeactivatedState;

        private void Awake()
        {
            m_Animator = GetComponentInChildren<Animator>();
            ActivatedState = new LeverState.Activated(this);
            DeactivatedState = new LeverState.Deactivated(this);
            StateMachine = new StateMachine<LeverState, Trigger>(DeactivatedState);

            StateMachine.Configure(DeactivatedState)
                .Permit(Trigger.Activate, ActivatedState);

            StateMachine.Configure(ActivatedState)
                .Permit(Trigger.Deactivate, DeactivatedState);
        }

        public enum Trigger
        {
            Activate,
            Deactivate
        }

        private System.Collections.IEnumerator InvokeWithDelay(Action action)
        {
            yield return new WaitForSeconds(m_ActivationDelay);
            action?.Invoke();
        }

        public bool IsActivated => StateMachine.State is LeverState.Activated;

        public void Activate()
        {
            if (StateMachine.CanFire(Trigger.Activate))
                StateMachine.Fire(Trigger.Activate);
        }

        public void Deactivate()
        {
            if (StateMachine.CanFire(Trigger.Deactivate))
                StateMachine.Fire(Trigger.Deactivate);
        }

        public void Toggle()
        {
            if (IsActivated)
                Deactivate();
            else
                Activate();
        }

        public HitResult TakeHit(HitData hitData)
        {
            if (((1 << hitData.Attacker.layer) & m_WhoAllowedToHit) != 0)
            {
                Toggle();
                OnHit?.Invoke();
                EventBus<LeverHitEvent>.Raise(new LeverHitEvent { LeverPosition = transform.position });
                return HitResult.Hit;
            }
            return HitResult.Nothing;
        }

        protected abstract class LeverState : State, IState
        {
            public Lever Lever;
            protected LeverState(Lever lever) { Lever = lever; }

            public class Activated : LeverState
            {
                public Activated(Lever lever) : base(lever) { }
                public override void OnEnter()
                {
                    Lever.m_Animator.SetTrigger(Lever.AnimationTriggerActivate);
                    Lever.StartCoroutine(Lever.InvokeWithDelay(Lever.OnActivate.Invoke));
                }
            }

            public class Deactivated : LeverState
            {
                public Deactivated(Lever lever) : base(lever) { }
                public override void OnEnter()
                {
                    Lever.m_Animator.SetTrigger(Lever.AnimationTriggerActivate);
                    Lever.StartCoroutine(Lever.InvokeWithDelay(Lever.OnDeactivate.Invoke));
                }
            }
        }
    }

}