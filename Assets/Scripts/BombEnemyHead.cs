using UnityEngine;

public class BombEnemyHead : MonoBehaviour
{
    [SerializeField] private BombEnemy bombEnemy;
    [SerializeField] private float stompBounceForce = 14f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.BounceUpAfterStomp(stompBounceForce);
        }

        if (bombEnemy != null)
        {
            bombEnemy.Die();
        }
    }
}