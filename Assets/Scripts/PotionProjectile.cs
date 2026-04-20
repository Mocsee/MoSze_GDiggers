using UnityEngine;

public class PotionProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject slowZonePrefab;

    private Vector2 moveDirection;
    private float moveSpeed;
    private bool initialized;
    private bool hasExploded;

    private float slowPercent;
    private float slowDuration;
    private float slowZoneRadius;

    public void Initialize(Vector2 direction, float speed, float newSlowPercent, float newSlowDuration, float newSlowZoneRadius)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        slowPercent = newSlowPercent;
        slowDuration = newSlowDuration;
        slowZoneRadius = newSlowZoneRadius;
        initialized = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialized || hasExploded) return;

        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryExplode(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryExplode(collision.gameObject);
    }

    private void TryExplode(GameObject other)
    {
        if (hasExploded) return;
        if (other == null) return;

        if (other.CompareTag("Ground") || other.CompareTag("Platform") || other.CompareTag("Player"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Vector3 explosionPosition = transform.position;

        if (slowZonePrefab != null)
        {
            GameObject zoneObject = Instantiate(slowZonePrefab, explosionPosition, Quaternion.identity);
            SlowZone slowZone = zoneObject.GetComponent<SlowZone>();

            if (slowZone != null)
            {
                slowZone.Initialize(slowPercent, slowDuration, slowZoneRadius);
            }
        }

        Destroy(gameObject);
    }
}