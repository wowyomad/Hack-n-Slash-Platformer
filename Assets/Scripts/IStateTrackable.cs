using System;

public interface IStateTrackable
{
    //void ChangeState(from, to);
    public event Action<IState, IState> StateChanged;
}