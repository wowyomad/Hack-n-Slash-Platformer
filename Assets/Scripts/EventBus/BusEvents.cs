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

    public struct LevelFinishTriggeredEvent : IEvent
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

    public struct LevelStartedEvent : IEvent
    {
        public Level Level;
    }

    public struct LevelCompletedEvent : IEvent
    {
        public Level Level;
    }
    public struct LevelFailedEvent : IEvent
    {
        public Level Level;
    }
    public struct LevelRestartedEvent : IEvent
    {
        public Level Level;
    }
    public struct LevelExitedEvent : IEvent
    {
        public Level Level;
    }

    public struct LevelTimeExpiredEvent : IEvent
    {
        public Level Level;
    }

    public struct EnemyAlertedEvent : IEvent
    {
        public bool Alerted;
        public GameObject EnemyGameObject;
    }
}
