using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBehaviorConfig", menuName = "Enemy/Config", order = 0)]
public class EnemyBehaviorConfigSO : ScriptableObject
{
    public float PatrolSpeed = 6.0f;
    public float SeekSpeed = 8.5f;

    public float VisualSeekDistance = 12.0f;
    public float CloseSeekDistance = 5.0f;

    public float SeekReachDistance = 0.5f;
}