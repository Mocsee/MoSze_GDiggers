using UnityEngine;

public class BombProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;

    private Vector2 moveDirection;
    private float moveSpeed;
    private bool initialized;

    public void Initialize(Vector2 direction, float speed)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        initialized = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialized) return;

        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(transform.position);

            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Platform"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}