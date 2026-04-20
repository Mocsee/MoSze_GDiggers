using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float sprintSpeed = 13f;
    [SerializeField] private float jumpForce = 18f;

    [Header("Gravity")]
    [SerializeField] private float normalGravity = 3f;
    [SerializeField] private float fallGravity = 6f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCheckDistance = 0.08f;
    [SerializeField] private float groundCheckWidthMultiplier = 0.9f;

    [Header("Jump Assist")]
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float wrapPadding = 0.5f;
    [SerializeField] private float wrapActivationDelay = 1f;

    [Header("Death")]
    [SerializeField] private float deathOffsetBelowCamera = 5f;
    [SerializeField] private float deathCheckDelay = 10f;

    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    private float timeSinceSceneLoad;
    private Vector3 startingPosition;
    private bool canMove = true;
    private float coyoteCounter;
    private float speedMultiplier = 1f;
    private Coroutine slowCoroutine;

    public bool IsSprinting { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        startingPosition = transform.position;

        if (mainCamera == null)
            mainCamera = Camera.main;

        groundLayers = LayerMask.GetMask("Ground", "Platform");
    }

    private void Start()
    {
        transform.position = startingPosition;
    }

    private void Update()
    {
        timeSinceSceneLoad += Time.deltaTime;

        bool grounded = IsGrounded();

        if (grounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        float xInput = Input.GetAxisRaw("Horizontal");

        if (canMove)
        {
            IsSprinting = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(xInput) > 0.01f;

            float baseSpeed = IsSprinting ? sprintSpeed : moveSpeed;
            float currentSpeed = baseSpeed * speedMultiplier;

            body.linearVelocity = new Vector2(xInput * currentSpeed, body.linearVelocity.y);

            if (Input.GetKeyDown(KeyCode.Space) && coyoteCounter > 0f)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
                coyoteCounter = 0f;
            }
        }

        if (body.linearVelocity.y < 0f)
            body.gravityScale = fallGravity;
        else
            body.gravityScale = normalGravity;
    }

    private void LateUpdate()
    {
        WrapHorizontally();
        CheckIfFellBelowCamera();
    }

    private bool IsGrounded()
    {
        if (boxCollider == null) return false;

        Bounds bounds = boxCollider.bounds;

        float castWidth = bounds.size.x * groundCheckWidthMultiplier;
        Vector2 boxCastSize = new Vector2(castWidth, bounds.size.y);
        Vector2 boxCastOrigin = bounds.center;

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCastOrigin,
            boxCastSize,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayers
        );

        return hit.collider != null;
    }

    private void WrapHorizontally()
    {
        if (mainCamera == null) return;
        if (timeSinceSceneLoad < wrapActivationDelay) return;

        float camHeight = mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;
        float camX = mainCamera.transform.position.x;

        float halfPlayerWidth = 0.5f;
        if (boxCollider != null)
            halfPlayerWidth = boxCollider.bounds.extents.x;

        float leftBound = camX - camWidth - halfPlayerWidth;
        float rightBound = camX + camWidth + halfPlayerWidth;

        Vector3 pos = transform.position;

        if (pos.x > rightBound + wrapPadding)
        {
            pos.x = leftBound + wrapPadding;
            transform.position = pos;
        }
        else if (pos.x < leftBound - wrapPadding)
        {
            pos.x = rightBound - wrapPadding;
            transform.position = pos;
        }
    }

    private void CheckIfFellBelowCamera()
    {
        if (mainCamera == null) return;
        if (timeSinceSceneLoad < deathCheckDelay) return;

        float cameraBottom = mainCamera.transform.position.y - mainCamera.orthographicSize;
        float deathY = cameraBottom - deathOffsetBelowCamera;

        if (transform.position.y < deathY)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void DisableMovementTemporarily(float duration)
    {
        StartCoroutine(DisableMovementCoroutine(duration));
    }

    private IEnumerator DisableMovementCoroutine(float duration)
    {
        canMove = false;
        yield return new WaitForSeconds(duration);
        canMove = true;
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        if (slowCoroutine != null)
            StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(ApplySlowCoroutine(slowPercent, duration));
    }

    private IEnumerator ApplySlowCoroutine(float slowPercent, float duration)
    {
        speedMultiplier = Mathf.Clamp(1f - slowPercent, 0.05f, 1f);
        yield return new WaitForSeconds(duration);
        speedMultiplier = 1f;
        slowCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D bc = GetComponent<BoxCollider2D>();
        if (bc != null)
        {
            Gizmos.color = Color.red;

            Bounds bounds = bc.bounds;
            float castWidth = bounds.size.x * groundCheckWidthMultiplier;
            Vector3 boxSize = new Vector3(castWidth, bounds.size.y, 0f);
            Vector3 boxCenter = bounds.center + Vector3.down * groundCheckDistance;

            Gizmos.DrawWireCube(boxCenter, boxSize);
        }
    }
}