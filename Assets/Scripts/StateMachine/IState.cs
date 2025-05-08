
namespace TheGame
{
    public interface IState
    {
        void Enter(IState from);
        void Exit();
        void Update();
        void FixedUpdate();
    }
}

