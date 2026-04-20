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

    private bool playerInRange = false;
    private bool isTalking = false;
    private Coroutine dialogueCoroutine;

    private void Start()
    {
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

        speechBubbleObject.SetActive(true);

        for (int i = 0; i < dialogueLines.Length; i++)
        {
            speechText.text = dialogueLines[i];
            yield return new WaitForSeconds(3f);
        }

        speechText.text = "";
        speechBubbleObject.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerInRange && interactPromptObject != null)
            interactPromptObject.SetActive(true);

        isTalking = false;
        dialogueCoroutine = null;
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
