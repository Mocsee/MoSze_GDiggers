using System.Collections;
using TMPro;
using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines;        // A párbeszéd sorai, amiket egymás után kiírunk

    [SerializeField] private TextMeshPro speechText;          // A szövegbuborékban megjelenő szöveg komponense
    [SerializeField] private GameObject speechBubbleObject;   // Maga a szövegbuborék (beszéd közben be-/kikapcsoljuk)
    [SerializeField] private PlayerMovement playerMovement;   // A játékos mozgásszkriptje (beszéd alatt letiltjuk)
    [SerializeField] private GameObject interactPromptObject; // A "Nyomd meg az E-t" felirat objektuma

    [Header("Highlight")]
    [SerializeField] private SpriteRenderer npcSprite;        // Az NPC képe (kiemeléskor átszínezzük)
    [SerializeField] private Color normalColor = Color.white; // Alap szín (amikor a játékos nincs a közelben)
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.7f, 1f); // Kiemelt szín (amikor a játékos a közelben van)

    [Header("Behaviour")]
    [SerializeField] private bool disappearAfterDialogue = false; // Eltűnjön-e az NPC a beszélgetés után

    [Header("Dialogue Flow")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;  // Ezzel a gombbal lehet a következő sorra ugrani (átléptetés)
    [SerializeField] private float lineDuration = 3f;         // Mennyi ideig látszódjon egy sor magától, ha nem nyomnak gombot

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private bool playerInRange = false;     // A közelben van-e a játékos
    private bool isTalking = false;         // Folyamatban van-e éppen a párbeszéd
    private bool isFairy = false;           // Tündér-e ez az NPC (a tündér beszéd után is a pályán marad)
    private Coroutine dialogueCoroutine;    // A futó párbeszéd-folyamat (coroutine), hogy meg tudjuk szakítani

    // --- INITIALIZATION (Inicializálás) ---
    private void Start()
    {
        // A tündérek a hangulatért lebegnek, és beszéd után sem tűnnek el
        isFairy = GetComponent<FairyFloat>() != null;

        // Induláskor elrejtjük a szövegbuborékot
        if (speechBubbleObject != null)
            speechBubbleObject.SetActive(false);

        // Induláskor elrejtjük az interakciós feliratot is
        if (interactPromptObject != null)
            interactPromptObject.SetActive(false);

        // Ha nem adtunk meg sprite-ot, megpróbáljuk a sajátunkat használni
        if (npcSprite == null)
            npcSprite = GetComponent<SpriteRenderer>();

        // Beállítjuk az alap színt
        if (npcSprite != null)
            npcSprite.color = normalColor;
    }

    // --- FŐ LOGIKA (a párbeszéd indítása) ---
    private void Update()
    {
        // Ha a játékos a közelben van, épp nem beszélünk, és megnyomja az E gombot -> indul a párbeszéd
        if (playerInRange && !isTalking && Input.GetKeyDown(KeyCode.E))
        {
            // Biztonsági ellenőrzések: legyen mit kiírni és legyen hová
            if (dialogueLines == null || dialogueLines.Length == 0) return;
            if (speechText == null || speechBubbleObject == null) return;

            // Beszéd közben elrejtjük az interakciós feliratot
            if (interactPromptObject != null)
                interactPromptObject.SetActive(false);

            // Elindítjuk a párbeszéd-folyamatot (coroutine)
            dialogueCoroutine = StartCoroutine(PlayDialogue());
        }
    }

    // --- A PÁRBESZÉD LEFUTÁSA (coroutine, soronként) ---
    private IEnumerator PlayDialogue()
    {
        isTalking = true;

        // Beszéd alatt letiltjuk a játékos mozgását
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Lefagyasztjuk az egész világot (ellenségek, lövedékek, fizika, díszlet), amíg beszélünk.
        // A Time.timeScale = 0 erre a legbiztosabb mód, mert minden időfüggő dolog megáll tőle.
        Time.timeScale = 0f;

        // Megjelenítjük a szövegbuborékot
        speechBubbleObject.SetActive(true);

        // Soronként végigmegyünk a párbeszéden
        for (int i = 0; i < dialogueLines.Length; i++)
        {
            speechText.text = dialogueLines[i];

            // Először várunk egy képkockát, hogy az előző sort továbbléptető gombnyomás
            // ne ugorja át rögtön ezt a sort is.
            yield return null;

            // Várunk, amíg vagy letelik a sor ideje, vagy a játékos megnyomja az átléptető gombot.
            // Fontos: a Time.unscaledDeltaTime-ot használjuk, mert a timeScale = 0 miatt a sima idő áll.
            float elapsed = 0f;
            while (elapsed < lineDuration && !Input.GetKeyDown(skipKey))
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // A párbeszéd végén kitöröljük a szöveget és elrejtjük a buborékot
        speechText.text = "";
        speechBubbleObject.SetActive(false);

        // Visszaindítjuk a világot, mert vége a beszélgetésnek
        Time.timeScale = 1f;

        // Visszaengedjük a játékos mozgását
        if (playerMovement != null)
            playerMovement.enabled = true;

        isTalking = false;
        dialogueCoroutine = null;

        // Ha be van állítva, hogy beszéd után tűnjön el (és NEM tündér), akkor megsemmisítjük
        if (disappearAfterDialogue && !isFairy)
        {
            if (interactPromptObject != null)
                interactPromptObject.SetActive(false);

            Destroy(gameObject);
            yield break;
        }

        // Ha a játékos még mindig a közelben van, újra megjelenítjük az interakciós feliratot
        if (playerInRange && interactPromptObject != null)
            interactPromptObject.SetActive(true);
    }

    // --- AMIKOR A JÁTÉKOS A KÖZELBE ÉR ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // Ha épp nem beszélünk, megjelenítjük az interakciós feliratot
            if (!isTalking && interactPromptObject != null)
                interactPromptObject.SetActive(true);

            // Ha még nincs meg a játékos mozgásszkriptje, elkérjük tőle
            if (playerMovement == null)
                playerMovement = other.GetComponent<PlayerMovement>();

            // Kiemeljük az NPC-t a kiemelő színnel
            if (npcSprite != null)
                npcSprite.color = highlightColor;
        }
    }

    // --- AMIKOR A JÁTÉKOS ELTÁVOLODIK ---
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Ha közben futott a párbeszéd, megszakítjuk
            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
                dialogueCoroutine = null;

                // A coroutine megszakadt, mielőtt visszaindította volna a világot, ezért itt tesszük meg
                Time.timeScale = 1f;
            }

            // Letöröljük a szöveget
            if (speechText != null)
                speechText.text = "";

            // Elrejtjük a szövegbuborékot
            if (speechBubbleObject != null)
                speechBubbleObject.SetActive(false);

            // Elrejtjük az interakciós feliratot
            if (interactPromptObject != null)
                interactPromptObject.SetActive(false);

            // Visszaengedjük a játékos mozgását
            if (playerMovement != null)
                playerMovement.enabled = true;

            // Visszaállítjuk az alap színt
            if (npcSprite != null)
                npcSprite.color = normalColor;

            isTalking = false;
        }
    }
}
