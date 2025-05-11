public class PlayerDeadState : PlayerBaseState
{
    public PlayerDeadState(Player player) : base(player) { }

    public override void OnEnter()
    {
        //Die animation, show death screen.
    }
}
