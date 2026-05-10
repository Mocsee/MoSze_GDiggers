using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerMovement))]
public class FallDamage : MonoBehaviour
{
    [Header("Fall Damage")]
    [SerializeField] private float distancePerHeart = 8f;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    private float highestY;
    private bool wasGrounded = true;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        highestY = transform.position.y;
    }

    private void Update()
    {
        if (distancePerHeart <= 0f) return;

        bool grounded = playerMovement.IsGrounded();

        if (!grounded)
        {
            if (transform.position.y > highestY)
                highestY = transform.position.y;
        }
        else
        {
            if (!wasGrounded)
            {
                float fallen = highestY - transform.position.y;
                int hearts = Mathf.FloorToInt(fallen / distancePerHeart);
                if (hearts > 0)
                    playerHealth.TakeFallDamage(hearts);
            }
            highestY = transform.position.y;
        }

        wasGrounded = grounded;
    }
}
