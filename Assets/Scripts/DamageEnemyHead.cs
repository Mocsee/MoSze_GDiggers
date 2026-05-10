using UnityEngine;

public class DamageEnemyHead : MonoBehaviour
{
    [SerializeField] private DamageEnemy damageEnemy;
    [SerializeField] private float stompBounceForce = 14f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.BounceUpAfterStomp(stompBounceForce);
        }

        if (damageEnemy != null)
        {
            damageEnemy.Die();
        }
    }
}
