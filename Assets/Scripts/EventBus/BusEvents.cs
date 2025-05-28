using UnityEngine;

namespace TheGame
{
    public struct TestEvent : IEvent
    {
        public string message;
    }

    public struct PlayerDiedEvent : IEvent
    {

    }

    public struct LevelFinishReachedEvent : IEvent
    {
        
    }

    public struct TriggerRestartEvent : IEvent
    {
        public GameObject Source;
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

    public struct LeverHitEvent : IEvent
    {
        public Vector3 LeverPosition;
    }
}
