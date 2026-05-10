using UnityEngine;

public class BombEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 10f;

    [Header("Bomb Throwing")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bombCooldown = 2f;
    [SerializeField] private float bombSpeed = 6f;

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
    private float bombTimer;
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
        {
            player = playerObject.transform;
            IgnorePlayerCollisions(playerObject);
        }

        bombTimer = bombCooldown;

        if (firePoint != null)
            firePointOriginalLocalPosition = firePoint.localPosition;
    }

    private void IgnorePlayerCollisions(GameObject playerObject)
    {
        Collider2D[] enemyColliders = GetComponentsInChildren<Collider2D>();
        Collider2D[] playerColliders = playerObject.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            if (enemyCollider.isTrigger) continue;
            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider.isTrigger) continue;
                Physics2D.IgnoreCollision(enemyCollider, playerCollider, true);
            }
        }
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
            HandleBombThrowing();
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

    private void HandleBombThrowing()
    {
        bombTimer -= Time.deltaTime;

        if (bombTimer <= 0f)
        {
            ThrowBomb();
            bombTimer = bombCooldown;
        }
    }

    private void ThrowBomb()
    {
        if (bombPrefab == null || firePoint == null || player == null) return;

        GameObject bombObject = Instantiate(bombPrefab, firePoint.position, Quaternion.identity);
        BombProjectile bomb = bombObject.GetComponent<BombProjectile>();

        if (bomb != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            bomb.Initialize(direction, bombSpeed);
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