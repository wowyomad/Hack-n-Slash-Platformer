
namespace TheGame
{
    public interface IState
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
        void FixedUpdate();
    }
}

