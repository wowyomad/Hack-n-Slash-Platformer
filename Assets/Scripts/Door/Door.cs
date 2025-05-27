using UnityEngine;

namespace TheGame
{

    [SelectionBase]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Door : MonoBehaviour
    {
        public bool IsOpened => StateMachine.State is DoorState.Opened;
        protected StateMachine<DoorState, Trigger> StateMachine;
        protected DoorState.Opened OpenedState;
        protected DoorState.Closed ClosedState;
        [SerializeField] protected string AnimationTriggerOpen = "Open";
        [SerializeField] protected string AnimationTriggerClose = "Close";

        private Animator m_Animator;
        private Collider2D m_Collider;

        protected bool Close()
        {
            if (StateMachine.CanFire(Trigger.Close))
            {
                StateMachine.Fire(Trigger.Close);
                return true;
            }
            return false;
        }

        protected bool Open()
        {
            if (StateMachine.CanFire(Trigger.Open))
            {
                StateMachine.Fire(Trigger.Open);
                return true;
            }
            return false;
        }

        protected virtual void Awake()
        {
            m_Animator = GetComponentInChildren<Animator>();
            m_Collider = GetComponent<Collider2D>();

            OpenedState = new DoorState.Opened(this);
            ClosedState = new DoorState.Closed(this);
            StateMachine = new StateMachine<DoorState, Trigger>(ClosedState);

            StateMachine.Configure(ClosedState)
                .Permit(Trigger.Open, OpenedState);

            StateMachine.Configure(OpenedState)
                .Permit(Trigger.Close, ClosedState);
        }

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
                    Animator.SetTrigger(Door.AnimationTriggerOpen);
                    Collider.enabled = false;
                }
            }
            public class Closed : DoorState
            {
                public Closed(Door door) : base(door) { }
                public override void OnEnter()
                {
                    Animator.SetTrigger(Door.AnimationTriggerClose);
                    Collider.enabled = true;
                }
            }
        }

        protected enum Trigger
        {
            Open,
            Close
        }
    }

}
