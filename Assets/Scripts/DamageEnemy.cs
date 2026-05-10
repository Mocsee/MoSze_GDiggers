using UnityEngine;

public class DamageEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 10f;

    [Header("Projectile Throwing")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float throwCooldown = 2f;
    [SerializeField] private float projectileSpeed = 6f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite[] movingSprites;
    [SerializeField] private float framesPerSecond = 10f;

    [Header("Pushback")]
    [SerializeField] private float pushStrength = 4f;
    [SerializeField] private float pushDamping = 8f;
    [SerializeField] private float pushCooldown = 0.15f;

    private Transform player;
    private float throwTimer;
    private bool isDead;
    private float pushCooldownTimer;
    private Vector3 firePointOriginalLocalPosition;
    private Vector2 pushVelocity;
    private bool isMoving;
    private float animationTimer;
    private int animationIndex;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;

        throwTimer = throwCooldown;

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
        UpdateSprite();
    }

    private void HandleBehaviour()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            MoveTowardsPlayer();
            HandleProjectileThrowing();
            UpdateFacingDirection();
            isMoving = true;
        }
        else
        {
            isMoving = false;
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
            spriteRenderer.flipX = false;

            if (firePoint != null)
                firePoint.localPosition = new Vector3(-Mathf.Abs(firePointOriginalLocalPosition.x), firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
        }
        else if (player.position.x > transform.position.x)
        {
            spriteRenderer.flipX = true;

            if (firePoint != null)
                firePoint.localPosition = new Vector3(Mathf.Abs(firePointOriginalLocalPosition.x), firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
        }
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        if (isMoving && movingSprites != null && movingSprites.Length > 0)
        {
            animationTimer += Time.deltaTime;
            float frameDuration = framesPerSecond > 0f ? 1f / framesPerSecond : 0.1f;
            if (animationTimer >= frameDuration)
            {
                animationTimer -= frameDuration;
                animationIndex = (animationIndex + 1) % movingSprites.Length;
            }
            spriteRenderer.sprite = movingSprites[animationIndex];
        }
        else
        {
            animationTimer = 0f;
            animationIndex = 0;
            if (idleSprite != null)
                spriteRenderer.sprite = idleSprite;
        }
    }

    private void HandleProjectileThrowing()
    {
        throwTimer -= Time.deltaTime;

        if (throwTimer <= 0f)
        {
            ThrowProjectile();
            throwTimer = throwCooldown;
        }
    }

    private void ThrowProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null) return;

        GameObject projectileObject = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        DamageProjectile projectile = projectileObject.GetComponent<DamageProjectile>();

        if (projectile != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            projectile.Initialize(direction, projectileSpeed);
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
