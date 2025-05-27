using UnityEngine;

namespace TheGame
{
    public struct TestEvent : IEvent
    {
        public string message;
    }

    public struct EnemyGotHitEvent : IEvent
    {
        public Vector3 EnemyPosition;
    }

    public struct EnemyGotParriedEvent : IEvent
    {
        public Vector3 EnemyPosition;
    }

    public struct PlayerGotHitEvent : IEvent
    {
        public Vector3 PlayerPosition;
    }

    public struct DoorOpenedWithHitEvent : IEvent
    {
        public Vector3 DoorPosition;
    }
}
