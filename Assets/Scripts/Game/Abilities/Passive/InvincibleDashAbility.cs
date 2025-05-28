
using System;
using System.IO;
using UnityEngine;

namespace TheGame
{
    [CreateAssetMenu(fileName = "InvincibleDash", menuName = "Game/Abilities/InvincibleDash")]
    public class InvincibleDashAbility : Ability
    {
        [NonSerialized] public Player Player;
        public override void Apply(Player player)
        {
            Player = player;
            player.WhenDash += OnDashPerfomred;
        }

        public override void Remove(Player player)
        {
            Player = null;
            player.WhenDash -= OnDashPerfomred;
        }

        private void OnDashPerfomred(bool performed)
        {
            if (!Player) return;

            if (performed)
            {
                OnDashStarted();
            }
            else
            {
                OnDashFinished();
            }
        }

        private void OnDashStarted()
        {
            Player.IsVulnerable = false;
        }

        private void OnDashFinished()
        {
            Player.IsVulnerable = true;
        }
    }
}
