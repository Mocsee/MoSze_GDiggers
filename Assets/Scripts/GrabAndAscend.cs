using UnityEngine;

public class GrabAndAscend : MonoBehaviour
{
    [Header("Grabbed Object")]
    [SerializeField] private Transform grabbedObject;
    [SerializeField] private Vector3 grabOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Ascension")]
    [SerializeField] private float startDelay = 0.5f;
    [SerializeField] private float ascendSpeed = 3f;
    [SerializeField] private float disappearAfterSeconds = 5f;

    [Header("Disappearance")]
    [SerializeField] private bool destroyGrabbedObject = true;
    [SerializeField] private float fadeDuration = 0.5f;

    private float elapsed;
    private bool ascending;
    private bool fading;
    private float fadeTimer;

    private SpriteRenderer selfRenderer;
    private SpriteRenderer grabbedRenderer;
    private Color selfBaseColor;
    private Color grabbedBaseColor;
    private Rigidbody2D grabbedRigidbody;
    private RigidbodyType2D grabbedOriginalBodyType;
    private bool grabbedHadRigidbody;

    private void Start()
    {
        selfRenderer = GetComponent<SpriteRenderer>();
        if (selfRenderer != null)
            selfBaseColor = selfRenderer.color;

        if (grabbedObject != null)
        {
            grabbedRigidbody = grabbedObject.GetComponent<Rigidbody2D>();
            if (grabbedRigidbody != null)
            {
                grabbedHadRigidbody = true;
                grabbedOriginalBodyType = grabbedRigidbody.bodyType;
                grabbedRigidbody.linearVelocity = Vector2.zero;
                grabbedRigidbody.angularVelocity = 0f;
                grabbedRigidbody.bodyType = RigidbodyType2D.Kinematic;
            }

            grabbedObject.SetParent(transform, false);
            grabbedObject.localPosition = grabOffset;
            grabbedObject.localRotation = Quaternion.identity;

            grabbedRenderer = grabbedObject.GetComponent<SpriteRenderer>();
            if (grabbedRenderer != null)
                grabbedBaseColor = grabbedRenderer.color;
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (!ascending)
        {
            if (elapsed >= startDelay)
                ascending = true;
            else
                return;
        }

        transform.position += Vector3.up * ascendSpeed * Time.deltaTime;

        if (!fading && elapsed >= startDelay + disappearAfterSeconds)
            fading = true;

        if (fading)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (fadeTimer / fadeDuration));

            if (selfRenderer != null)
                selfRenderer.color = new Color(selfBaseColor.r, selfBaseColor.g, selfBaseColor.b, alpha * selfBaseColor.a);

            if (grabbedRenderer != null)
                grabbedRenderer.color = new Color(grabbedBaseColor.r, grabbedBaseColor.g, grabbedBaseColor.b, alpha * grabbedBaseColor.a);

            if (fadeTimer >= fadeDuration)
            {
                if (destroyGrabbedObject && grabbedObject != null)
                    Destroy(grabbedObject.gameObject);

                Destroy(gameObject);
            }
        }
    }

    private void LateUpdate()
    {
        if (grabbedObject == null) return;

        grabbedObject.localPosition = grabOffset;

        if (grabbedRigidbody != null)
        {
            grabbedRigidbody.linearVelocity = Vector2.zero;
            grabbedRigidbody.angularVelocity = 0f;
        }
    }
}
