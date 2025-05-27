using System;
using UnityEngine;

namespace TheGame
{
    public class Door : MonoBehaviour, IHittable
    {
        public event Action OnHit;

        private Animator m_Animator;
        private Collider2D m_Collider;

        public abstract class DoorState : State, IState
        {
            public Door Door;
            public Animator Animator => Door.m_Animator;
            public Collider2D Collider => Door.m_Collider;
            public DoorState(Door door)
            {
                Door = door;
            }
            public class Opened : DoorState
            {
                public Opened(Door door) : base(door) { }
                public override void OnEnter()
                {
                    Animator.SetTrigger("Open");
                    Collider.enabled = false;
                }
            }
            public class Closed : DoorState
            {
                public Closed(Door door) : base(door) { }
                public override void OnEnter()
                {
                    Animator.SetTrigger("Close");
                    Collider.enabled = true;
                }
            }
        }


        public enum Trigger
        {
            Open,
            Close
        }

        [SerializeField] private LayerMask m_WhoAllowedToHit;

        private StateMachine<DoorState, Trigger> m_StateMachine;
        private DoorState.Opened m_OpenedState;
        private DoorState.Closed m_ClosedState;

        private void Awake()
        {
            m_Animator = GetComponentInChildren<Animator>();
            m_Collider = GetComponent<Collider2D>();

            m_OpenedState = new DoorState.Opened(this);
            m_ClosedState = new DoorState.Closed(this);
            m_StateMachine = new StateMachine<DoorState, Trigger>(m_ClosedState);

            m_StateMachine.Configure(m_ClosedState)
                .Permit(Trigger.Open, m_OpenedState);

            m_StateMachine.Configure(m_OpenedState)
                .Permit(Trigger.Close, m_ClosedState);
        }

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public HitResult TakeHit(HitData hitData)
        {
            if (((1 << hitData.Attacker.layer) & m_WhoAllowedToHit) != 0)
            {
                if (m_StateMachine.CanFire(Trigger.Open))
                {
                    m_StateMachine.Fire(Trigger.Open);
                    OnHit?.Invoke();
                    EventBus<DoorOpenedWithHitEvent>.Raise(new DoorOpenedWithHitEvent
                    {
                        DoorPosition = transform.position
                    });
                }
            }
            return HitResult.Nothing;
        }
    }

}
