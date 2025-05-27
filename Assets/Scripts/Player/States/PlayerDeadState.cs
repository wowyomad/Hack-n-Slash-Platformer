using UnityEngine;

namespace TheGame
{
    public class PlayerDeadState : PlayerBaseState
    {
        public PlayerDeadState(Player player) : base(player) { }

        public override void OnEnter()
        {
            Debug.Log("Player is dead");

            Controller.Velocity.x = 0.0f;
        }
    }
}
