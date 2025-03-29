using UnityEngine;

public class TestEventListener : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus<TestEvent>.OnEvent += OnTestEvent;
    }
    private void OnDisable()
    {
        EventBus<TestEvent>.OnEvent -= OnTestEvent;
    }
    private void OnTestEvent(TestEvent @event)
    {
        Debug.Log($"Received event: {@event.message}");
    }
}