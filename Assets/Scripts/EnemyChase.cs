using System.Collections;
using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Chase")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 8f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 2.5f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private Transform player;
    private bool isDead = false;
    private bool isKnockedBack = false;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    private void Update()
    {
        if (isDead) return;
        if (isKnockedBack) return;
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(transform.position);
            KnockbackAwayFromPlayer(other.transform.position);
        }
    }

    private void KnockbackAwayFromPlayer(Vector3 playerPosition)
    {
        StartCoroutine(KnockbackCoroutine(playerPosition));
    }

    private IEnumerator KnockbackCoroutine(Vector3 playerPosition)
    {
        isKnockedBack = true;

        Vector3 direction = (transform.position - playerPosition).normalized;
        float timer = 0f;

        while (timer < knockbackDuration)
        {
            transform.position += direction * knockbackForce * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        isKnockedBack = false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Destroy(gameObject);
    }
}