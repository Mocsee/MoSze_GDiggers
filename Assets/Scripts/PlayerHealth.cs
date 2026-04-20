using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private Image[] lifeImages;

    [Header("Damage")]
    [SerializeField] private float invincibilityTime = 1f;
    [SerializeField] private float bounceForce = 12f;
    [SerializeField] private float movementLockTime = 0.25f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem damageEffect;

    private int currentLives;
    private bool isInvincible;
    private Rigidbody2D body;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        currentLives = maxLives;
        UpdateLivesUI();
    }

    public void TakeDamage(Vector2 enemyPosition)
    {
        if (isInvincible) return;

        currentLives--;
        UpdateLivesUI();
        PlayDamageEffect();

        Vector2 bounceDirection = ((Vector2)transform.position - enemyPosition).normalized;
        bounceDirection.y = Mathf.Abs(bounceDirection.y) + 0.5f;

        body.linearVelocity = Vector2.zero;
        body.AddForce(bounceDirection.normalized * bounceForce, ForceMode2D.Impulse);

        if (playerMovement != null)
            playerMovement.DisableMovementTemporarily(movementLockTime);

        if (currentLives <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibilityCoroutine());
    }

    public void BounceUpAfterStomp(float stompBounceForce)
    {
        body.linearVelocity = new Vector2(body.linearVelocity.x, stompBounceForce);
    }

    private void PlayDamageEffect()
    {
        if (damageEffect == null) return;

        damageEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        damageEffect.Clear();
        damageEffect.Play();
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }

    private void UpdateLivesUI()
    {
        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] != null)
                lifeImages[i].enabled = i < currentLives;
        }
    }

    private void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}