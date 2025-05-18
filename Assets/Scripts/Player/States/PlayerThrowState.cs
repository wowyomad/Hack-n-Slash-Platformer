namespace TheGame
{
    public class PlayerThrowState : PlayerBaseState
    {
        public PlayerThrowState(Player player) : base(player) { }

        public override void OnEnter()
        {
            Player.ThrowKnife();
        }
    }
}
