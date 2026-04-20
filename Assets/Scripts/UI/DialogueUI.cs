using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    private string[] currentLines;
    private int currentLineIndex;
    private bool isShowing;

    public bool IsShowing => isShowing;

    private void Start()
    {
        HideDialogue();
    }

    public void StartDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0) return;

        currentLines = lines;
        currentLineIndex = 0;
        isShowing = true;

        dialoguePanel.SetActive(true);
        dialogueText.text = currentLines[currentLineIndex];
    }

    public void ShowNextLine()
    {
        if (!isShowing || currentLines == null) return;

        currentLineIndex++;

        if (currentLineIndex >= currentLines.Length)
        {
            HideDialogue();
            return;
        }

        dialogueText.text = currentLines[currentLineIndex];
    }

    public void HideDialogue()
    {
        isShowing = false;
        currentLines = null;
        currentLineIndex = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";
    }
}
