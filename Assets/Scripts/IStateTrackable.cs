using System;
using TheGame;

public interface IStateTrackable
{
    public event Action<IState, IState> StateChanged;
}