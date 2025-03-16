using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine
{
    public PlayerState CurrentState { get; private set; }

    public void Initialize(PlayerState initialState)
    {
        CurrentState = initialState;
        CurrentState.Enter();
    }

    public void SwitchState(PlayerState state)
    {
        if (state == CurrentState)
            return;

        CurrentState.Exit();
        CurrentState = state;
        CurrentState.Enter();
    }

}
