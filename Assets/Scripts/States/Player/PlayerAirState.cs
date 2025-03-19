using UnityEngine;

public class PlayerAirState : PlayerBaseState
{
    InputReader m_Input;
    public PlayerAirState(Player player) : base(player) { }

    public override void OnEnter(IState from)
    {
        Debug.Log("Player entered Falling State");
    }

    public override void Update()
    {
        
    }
}