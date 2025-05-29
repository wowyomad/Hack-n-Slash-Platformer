using System.Collections.Generic;

namespace TheGame
{
    public interface IPicker<TItem> where TItem : class
    {
        List<IPickupable<TItem>> PickablesInArea { get; }
        public bool AnyPickableInArea => PickablesInArea.Count > 0;
    }
}