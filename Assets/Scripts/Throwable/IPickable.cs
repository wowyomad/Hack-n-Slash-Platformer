using UnityEngine;

namespace TheGame
{
    public interface IPickupable<TItem> where TItem : class
    {
        TItem Pickup(IPicker<TItem> picker);
        GameObject gameObject { get; }
    }
}
