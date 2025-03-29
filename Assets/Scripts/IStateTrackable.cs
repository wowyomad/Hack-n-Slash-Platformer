using System;

public interface IStateTrackable
{
    public event Action<IState, IState> OnStateChange;
}