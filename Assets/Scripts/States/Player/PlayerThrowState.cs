using TheGame;
using UnityEditor;

public class PlayerThrowState : PlayerBaseState
{
    public PlayerThrowState(Player player) : base(player) { }

    public override void Enter(IState from)
    {
        Player.ThrowKnife();
    }
} 