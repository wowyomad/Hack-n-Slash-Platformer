
namespace TheGame
{
    public interface IState
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
        void FixedUpdate();
    }

    public abstract class State
    {
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate() { }
        public virtual void FixedUpdate() { }
    }
}

