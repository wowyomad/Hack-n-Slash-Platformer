using UnityEngine;

namespace TheGame
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class FinishTrigger : MonoBehaviour
    {
        private LevelManager m_LevelManager;

        private void Awake()
        {

        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            TryCompleteLevel(other.GetComponent<Player>());
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryCompleteLevel(collision.collider.GetComponent<Player>());
        }

        private void TryCompleteLevel(Player player)
        {
            if (player == null) return;

            EventBus<LevelFinishTriggeredEvent>.Raise(new LevelFinishTriggeredEvent { });
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(1f, 1f, 0f));
        }
    }
}
