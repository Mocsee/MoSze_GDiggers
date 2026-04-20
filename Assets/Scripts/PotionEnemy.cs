using UnityEngine;

public class PotionEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 10f;

    [Header("Potion Throwing")]
    [SerializeField] private GameObject potionPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float potionCooldown = 2.5f;
    [SerializeField] private float potionSpeed = 5f;

    [Header("Slow Effect")]
    [SerializeField] private float slowPercent = 0.5f;
    [SerializeField] private float slowDuration = 2f;
    [SerializeField] private float slowZoneRadius = 1.5f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Pushback")]
    [SerializeField] private float pushStrength = 4f;
    [SerializeField] private float pushDamping = 8f;
    [SerializeField] private float pushCooldown = 0.15f;

    private Transform player;
    private float potionTimer;
    private bool isDead;
    private Vector3 firePointOriginalLocalPosition;

    private Vector2 pushVelocity;
    private float pushCooldownTimer;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;

        potionTimer = potionCooldown;

        if (firePoint != null)
            firePointOriginalLocalPosition = firePoint.localPosition;
    }

    private void Update()
    {
        if (isDead) return;
        if (player == null) return;

        if (pushCooldownTimer > 0f)
            pushCooldownTimer -= Time.deltaTime;

        UpdatePushback();
        HandleBehaviour();
    }

    private void HandleBehaviour()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            MoveTowardsPlayer();
            HandlePotionThrowing();
            UpdateFacingDirection();
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = player.position;

        Vector2 chaseDirection = (targetPosition - currentPosition).normalized;
        Vector2 chaseMovement = chaseDirection * moveSpeed * Time.deltaTime;

        Vector2 finalMovement = chaseMovement + (pushVelocity * Time.deltaTime);
        transform.position += (Vector3)finalMovement;
    }

    private void UpdatePushback()
    {
        pushVelocity = Vector2.Lerp(pushVelocity, Vector2.zero, pushDamping * Time.deltaTime);
    }

    private void UpdateFacingDirection()
    {
        if (spriteRenderer == null) return;

        if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true;

            if (firePoint != null)
                firePoint.localPosition = new Vector3(-Mathf.Abs(firePointOriginalLocalPosition.x), firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
        }
        else if (player.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false;

            if (firePoint != null)
                firePoint.localPosition = new Vector3(Mathf.Abs(firePointOriginalLocalPosition.x), firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
        }
    }

    private void HandlePotionThrowing()
    {
        potionTimer -= Time.deltaTime;

        if (potionTimer <= 0f)
        {
            ThrowPotion();
            potionTimer = potionCooldown;
        }
    }

    private void ThrowPotion()
    {
        if (potionPrefab == null || firePoint == null || player == null) return;

        GameObject potionObject = Instantiate(potionPrefab, firePoint.position, Quaternion.identity);
        PotionProjectile potion = potionObject.GetComponent<PotionProjectile>();

        if (potion != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            potion.Initialize(direction, potionSpeed, slowPercent, slowDuration, slowZoneRadius);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (pushCooldownTimer > 0f) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        Vector2 awayFromPlayer = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;

        if (awayFromPlayer == Vector2.zero)
            awayFromPlayer = Vector2.right;

        pushVelocity = awayFromPlayer * pushStrength;
        pushCooldownTimer = pushCooldown;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Destroy(gameObject);
    }
}