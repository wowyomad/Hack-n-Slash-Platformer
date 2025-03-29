using System;
using UnityEngine;

public interface IEvent
{

}

public struct TestEvent : IEvent
{
    public string message;
}

public struct EnemyHitEvent : IEvent
{
    public Vector3 EnemyPosition;
}

public struct PlayerHitEvent : IEvent
{
    public Vector3 PlayerPosition;
}

public class EventBus<T> where T : struct, IEvent
{
    public static event Action<T> OnEvent;
    public static void Raise(T @event) => OnEvent?.Invoke(@event);
}
