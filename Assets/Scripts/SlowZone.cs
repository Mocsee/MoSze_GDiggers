using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private ParticleSystem blueExplosionEffect;

    private float slowPercent = 0.5f;
    private float slowDuration = 2f;
    private float radius = 1.5f;
    private bool initialized;

    public void Initialize(float newSlowPercent, float newSlowDuration, float newRadius)
    {
        slowPercent = newSlowPercent;
        slowDuration = newSlowDuration;
        radius = newRadius;
        initialized = true;
        SetupZone();
    }

    private void Start()
    {
        if (!initialized)
            SetupZone();
    }

    private void SetupZone()
    {
        if (circleCollider != null)
            circleCollider.radius = radius;

        if (blueExplosionEffect != null)
        {
            blueExplosionEffect.transform.localPosition = Vector3.zero;
            blueExplosionEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            blueExplosionEffect.Clear();
            blueExplosionEffect.Play();
        }

        ApplySlowToPlayersInside();
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.ApplySlow(slowPercent, slowDuration);
        }
    }

    private void ApplySlowToPlayersInside()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].CompareTag("Player")) continue;

            PlayerMovement playerMovement = hits[i].GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ApplySlow(slowPercent, slowDuration);
            }
        }
    }
}