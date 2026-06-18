using UnityEngine;

public class FairyFloat : MonoBehaviour
{
    [Header("Floating")]
    [SerializeField] private float floatAmplitude = 0.25f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite tunder1;
    [SerializeField] private Sprite tunder2;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        float wave = Time.time * floatSpeed;

        float offsetY = Mathf.Sin(wave) * floatAmplitude;
        transform.position = startPosition + new Vector3(0f, offsetY, 0f);

        if (spriteRenderer != null && tunder1 != null && tunder2 != null)
        {
            // Cos is the slope of Sin: positive while rising, negative while falling.
            bool movingUp = Mathf.Cos(wave) >= 0f;
            spriteRenderer.sprite = movingUp ? tunder1 : tunder2;
        }
    }
}
