public interface IState
{
    void OnEnter(IState from);
    void OnExit();
    void Update();
    void FixedUpdate();
}
