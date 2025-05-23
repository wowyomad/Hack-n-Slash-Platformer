using System;

namespace TheGame
{
    public interface IEvent
    {

    }

    public class EventBus<T> where T : struct, IEvent
    {
        public static event Action<T> OnEvent;
        public static void Raise(T @event) => OnEvent?.Invoke(@event);
    }
}
