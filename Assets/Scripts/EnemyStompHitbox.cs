using UnityEngine;

public class EnemyStompHitbox : MonoBehaviour
{
    [SerializeField] private EnemyChase enemy;
    [SerializeField] private float stompBounceForce = 14f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody2D playerBody = other.GetComponent<Rigidbody2D>();
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerBody == null || playerHealth == null || enemy == null) return;

        if (playerBody.linearVelocity.y < 0f)
        {
            playerHealth.BounceUpAfterStomp(stompBounceForce);
            enemy.Die();
        }
    }
}