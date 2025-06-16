using UnityEngine;

namespace TheGame
{

    [CreateAssetMenu(fileName = "SheathAbility", menuName = "Game/Abilities/Sheath")]
    public class SheathAbility : Ability
    {
        public override void Apply(Player player)
        {
            player.CanPickupKnifes = true;
        }

        public override void Remove(Player player)
        {
            player.CanPickupKnifes = false;
        }
    }
}