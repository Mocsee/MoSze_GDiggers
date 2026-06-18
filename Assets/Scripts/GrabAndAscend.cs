using TMPro;
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

    [Header("Taunt")]
    [Tooltip("Optional. Leave empty to have one created automatically above this object.")]
    [SerializeField] private TextMeshPro tauntText;
    [SerializeField] private string tauntMessage = "Sose kapsz el!";
    [SerializeField] private Vector3 tauntOffset = new Vector3(0f, 6f, 0f);
    [SerializeField] private float tauntFontSize = 36f;
    [SerializeField] private Color tauntColor = Color.white;

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

    private bool tauntShown;
    private Color tauntBaseColor;

    private void Start()
    {
        selfRenderer = GetComponent<SpriteRenderer>();
        if (selfRenderer != null)
            selfBaseColor = selfRenderer.color;

        SetupTaunt();

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

            // The taunt fades away together with him.
            if (tauntText != null)
                tauntText.color = new Color(tauntBaseColor.r, tauntBaseColor.g, tauntBaseColor.b, alpha * tauntBaseColor.a);

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

    private void SetupTaunt()
    {
        // Build a floating label above him if one wasn't wired up in the inspector.
        if (tauntText == null)
        {
            GameObject textObject = new GameObject("TauntText");
            textObject.transform.SetParent(transform, false);

            tauntText = textObject.AddComponent<TextMeshPro>();
            tauntText.fontSize = tauntFontSize;
            tauntText.enableAutoSizing = false;
            tauntText.alignment = TextAlignmentOptions.Center;
            tauntText.rectTransform.sizeDelta = new Vector2(20f, 6f);

            MeshRenderer textRenderer = textObject.GetComponent<MeshRenderer>();
            if (textRenderer != null)
            {
                // Draw on top of his sprite.
                if (selfRenderer != null)
                    textRenderer.sortingLayerID = selfRenderer.sortingLayerID;
                textRenderer.sortingOrder = (selfRenderer != null ? selfRenderer.sortingOrder : 0) + 10;
            }
        }

        // Counteract this object's scale so the text isn't stretched by it.
        Vector3 lossy = transform.lossyScale;
        tauntText.transform.localScale = new Vector3(
            Mathf.Approximately(lossy.x, 0f) ? 1f : 1f / lossy.x,
            Mathf.Approximately(lossy.y, 0f) ? 1f : 1f / lossy.y,
            1f);
        tauntText.transform.localPosition = tauntOffset;

        tauntText.text = tauntMessage;
        tauntText.color = tauntColor;
        tauntBaseColor = tauntColor;

        // Hidden until the player gets near.
        tauntText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (tauntShown) return;
        if (!other.CompareTag("Player")) return;

        tauntShown = true;

        if (tauntText != null)
            tauntText.gameObject.SetActive(true);
    }
}
