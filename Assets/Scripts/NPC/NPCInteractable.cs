using System.Collections;
using TMPro;
using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines;

    [SerializeField] private TextMeshPro speechText;
    [SerializeField] private GameObject speechBubbleObject;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject interactPromptObject;

    [Header("Highlight")]
    [SerializeField] private SpriteRenderer npcSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.7f, 1f);

    [Header("Behaviour")]
    [SerializeField] private bool disappearAfterDialogue = false;

    [Header("Dialogue Flow")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private float lineDuration = 3f;

    private bool playerInRange = false;
    private bool isTalking = false;
    private bool isFairy = false;
    private Coroutine dialogueCoroutine;

    private void Start()
    {
        // Fairies float around for ambience and should stick around after talking.
        isFairy = GetComponent<FairyFloat>() != null;

        if (speechBubbleObject != null)
            speechBubbleObject.SetActive(false);

        if (interactPromptObject != null)
            interactPromptObject.SetActive(false);

        if (npcSprite == null)
            npcSprite = GetComponent<SpriteRenderer>();

        if (npcSprite != null)
            npcSprite.color = normalColor;
    }

    private void Update()
    {
        if (playerInRange && !isTalking && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueLines == null || dialogueLines.Length == 0) return;
            if (speechText == null || speechBubbleObject == null) return;

            if (interactPromptObject != null)
                interactPromptObject.SetActive(false);

            dialogueCoroutine = StartCoroutine(PlayDialogue());
        }
    }

    private IEnumerator PlayDialogue()
    {
        isTalking = true;

        if (playerMovement != null)
            playerMovement.enabled = false;

        // Freeze the whole world (enemies, projectiles, physics, ambience) while talking.
        Time.timeScale = 0f;

        speechBubbleObject.SetActive(true);

        for (int i = 0; i < dialogueLines.Length; i++)
        {
            speechText.text = dialogueLines[i];

            // Wait one unscaled frame first so the key press that advanced the
            // previous line doesn't instantly skip this one too.
            yield return null;

            float elapsed = 0f;
            while (elapsed < lineDuration && !Input.GetKeyDown(skipKey))
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        speechText.text = "";
        speechBubbleObject.SetActive(false);

        // Resume the world now that the conversation is over.
        Time.timeScale = 1f;

        if (playerMovement != null)
            playerMovement.enabled = true;

        isTalking = false;
        dialogueCoroutine = null;

        if (disappearAfterDialogue && !isFairy)
        {
            if (interactPromptObject != null)
                interactPromptObject.SetActive(false);

            Destroy(gameObject);
            yield break;
        }

        if (playerInRange && interactPromptObject != null)
            interactPromptObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (!isTalking && interactPromptObject != null)
                interactPromptObject.SetActive(true);

            if (playerMovement == null)
                playerMovement = other.GetComponent<PlayerMovement>();

            if (npcSprite != null)
                npcSprite.color = highlightColor;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
                dialogueCoroutine = null;

                // The coroutine was killed before it could unfreeze, so do it here.
                Time.timeScale = 1f;
            }

            if (speechText != null)
                speechText.text = "";

            if (speechBubbleObject != null)
                speechBubbleObject.SetActive(false);

            if (interactPromptObject != null)
                interactPromptObject.SetActive(false);

            if (playerMovement != null)
                playerMovement.enabled = true;

            if (npcSprite != null)
                npcSprite.color = normalColor;

            isTalking = false;
        }
    }
}
