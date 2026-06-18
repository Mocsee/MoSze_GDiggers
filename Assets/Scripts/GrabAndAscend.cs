using TMPro;
using UnityEngine;

public class GrabAndAscend : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    [Header("Grabbed Object")]
    [SerializeField] private Transform grabbedObject;       // Az objektum, amelyet elkapunk és felviszünk a magasba
    [SerializeField] private Vector3 grabOffset = new Vector3(0f, 0.5f, 0f); // Milyen eltolással (távolsággal) rögzítsük magunkhoz az elkapott tárgyat

    [Header("Ascension")]
    [SerializeField] private float startDelay = 0.5f;        // Késleltetés az elkapás után, mielőtt ténylegesen elindulna felfelé
    [SerializeField] private float ascendSpeed = 3f;        // A felfelé lebegés/emelkedés sebessége
    [SerializeField] private float disappearAfterSeconds = 5f; // Hány másodpercig emelkedjen, mielőtt elkezdene eltűnni

    [Header("Disappearance")]
    [SerializeField] private bool destroyGrabbedObject = true; // Megsemmisítse-e az elkapott tárgyat is a folyamat végén
    [SerializeField] private float fadeDuration = 0.5f;     // Az elhalványodás (áttetszőség nullára csökkenésének) időtartama

    [Header("Taunt")]
    [Tooltip("Optional. Leave empty to have one created automatically above this object.")]
    [SerializeField] private TextMeshPro tauntText;          // A gonosz fölött megjelenő gúnyolódó szöveg (ha üres, automatikusan létrehozzuk)
    [SerializeField] private string tauntMessage = "Sose kapsz el!"; // A kiírt gúnyolódó mondat
    [SerializeField] private Vector3 tauntOffset = new Vector3(0f, 6f, 0f); // A szöveg eltolása a karakter középpontjához képest (felfelé)
    [SerializeField] private float tauntFontSize = 36f;      // A szöveg betűmérete
    [SerializeField] private Color tauntColor = Color.white; // A szöveg színe

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private float elapsed;                  // Az elkapás óta eltelt összes idő másodpercben
    private bool ascending;                 // Elindult-e már a felfelé mozgás
    private bool fading;                    // Elindult-e már az elhalványodás
    private float fadeTimer;                // Időzítő az elhalványodási fázis pontos kiszámításához

    private SpriteRenderer selfRenderer;    // Saját képmegjelenítő komponens
    private SpriteRenderer grabbedRenderer; // Az elkapott tárgy képmegjelenítő komponense
    private Color selfBaseColor;            // Saját eredeti szín (az alfa/átlátszóság visszaállításához/módosításához)
    private Color grabbedBaseColor;         // Az elkapott tárgy eredeti színe
    private Rigidbody2D grabbedRigidbody;   // Az elkapott tárgy fizikai teste (ha van neki)
    private RigidbodyType2D grabbedOriginalBodyType; // Az elkapott tárgy eredeti fizikai típusa (Dynamic, Static, stb.)
    private bool grabbedHadRigidbody;       // Logikai változó: volt-e egyáltalán fizikai teste az elkapott tárgynak

    private bool tauntShown;                // Megjelent-e már a gúnyolódó szöveg (csak egyszer jelenhet meg)
    private Color tauntBaseColor;           // A gúnyolódó szöveg eredeti színe (az elhalványításhoz)

    // --- INITIALIZATION (Inicializálás az elkapás pillanatában) ---
    private void Start()
    {
        // Megkeressük a saját SpriteRendererünket és elmentjük az eredeti színét
        selfRenderer = GetComponent<SpriteRenderer>();
        if (selfRenderer != null)
            selfBaseColor = selfRenderer.color;

        // Létrehozzuk/előkészítjük a fölötte lebegő gúnyolódó szöveget
        SetupTaunt();

        // Ha van elkapott objektum, felkészítjük a felemelésre
        if (grabbedObject != null)
        {
            // Megnézzük, van-e fizikai teste az elkapott tárgynak
            grabbedRigidbody = grabbedObject.GetComponent<Rigidbody2D>();
            if (grabbedRigidbody != null)
            {
                grabbedHadRigidbody = true;
                grabbedOriginalBodyType = grabbedRigidbody.bodyType; // Elmentjük az eredeti fizikai beállítást

                // Lefékezzük a tárgyat: nullázzuk a sebességeit (haladási és forgási sebesség)
                grabbedRigidbody.linearVelocity = Vector2.zero;
                grabbedRigidbody.angularVelocity = 0f;

                // Kinematic-ra váltjuk a testét. Ez kikapcsolja rá a gravitációt és a külső ütközések hatásait,
                // így a tárgy nem fog leesni vagy elrepülni, miközben emeljük fel.
                grabbedRigidbody.bodyType = RigidbodyType2D.Kinematic;
            }

            // Unity hierarchia trükk: Az elkapott tárgyat ennek az objektumnak a "gyermekévé" tesszük.
            // Ez azt jelenti, hogy ha ez az objektum mozog, az elkapott tárgy automatikusan követni fogja.
            grabbedObject.SetParent(transform, false);

            // Beállítjuk az elkapott tárgy helyzetét és forgását a saját koordinátáinkhoz képest (lokálisan)
            grabbedObject.localPosition = grabOffset;
            grabbedObject.localRotation = Quaternion.identity; // Nullázza a forgatást (egyenesen fog állni)

            // Megkeressük az elkapott tárgy SpriteRendererét is az elhalványításhoz
            grabbedRenderer = grabbedObject.GetComponent<SpriteRenderer>();
            if (grabbedRenderer != null)
                grabbedBaseColor = grabbedRenderer.color;
        }
    }

    // --- FŐ LOGIKA (Minden képkockán) ---
    private void Update()
    {
        // Növeljük az eltelt időt
        elapsed += Time.deltaTime;

        // --- 1. FÁZIS: INDULÁS ELŐTTI VÁRAKOZÁS ---
        if (!ascending)
        {
            // Ha letelt a startDelay várakozási idő, átváltunk emelkedési fázisba
            if (elapsed >= startDelay)
                ascending = true;
            else
                return; // Ha még nem telt le, megállítjuk az Update-et, nem mozgunk felfelé
        }

        // --- 2. FÁZIS: EMELKEDÉS ---
        // Folyamatosan növeljük a pozíciót felfelé (Vector3.up) a megadott sebességgel
        transform.position += Vector3.up * ascendSpeed * Time.deltaTime;

        // --- 3. FÁZIS: ELHALVÁNYODÁS KAPCSOLÁSA ---
        // Ha még nem halványodunk, de letelt a várakozási idő + az emelkedési idő, bekapcsoljuk a halványodást
        if (!fading && elapsed >= startDelay + disappearAfterSeconds)
            fading = true;

        // --- 4. FÁZIS: ELHALVÁNYODÁS ÉS MEGSEMMISÍTÉS ---
        if (fading)
        {
            fadeTimer += Time.deltaTime; // Mérjük az elhalványodás óta eltelt időt

            // Kiszámítjuk az új alfa (átlátszósági) értéket 1 és 0 között.
            // Ahogy a fadeTimer nő, a kivonás miatt az érték 1-ről csökken 0-ig.
            // A Mathf.Clamp01 garantálja, hogy az érték szigorúan 0 és 1 között maradjon.
            float alpha = Mathf.Clamp01(1f - (fadeTimer / fadeDuration));

            // Ha nekünk van SpriteRendererünk, csökkentjük az átlátszóságát az új alfa értékkel
            if (selfRenderer != null)
                selfRenderer.color = new Color(selfBaseColor.r, selfBaseColor.g, selfBaseColor.b, alpha * selfBaseColor.a);

            // Ha az elkapott tárgynak van SpriteRenderere, azt is ugyanúgy elhalványítjuk
            if (grabbedRenderer != null)
                grabbedRenderer.color = new Color(grabbedBaseColor.r, grabbedBaseColor.g, grabbedBaseColor.b, alpha * grabbedBaseColor.a);

            // A gúnyolódó szöveg vele együtt halványodik el
            if (tauntText != null)
                tauntText.color = new Color(tauntBaseColor.r, tauntBaseColor.g, tauntBaseColor.b, alpha * tauntBaseColor.a);

            // Ha az elhalványodási idő teljesen letelt (az alfa elérte a 0-át)
            if (fadeTimer >= fadeDuration)
            {
                // Ha be van kapcsolva a törlés, és a tárgy még létezik, véglegesen töröljük a játékból
                if (destroyGrabbedObject && grabbedObject != null)
                    Destroy(grabbedObject.gameObject);

                // Végül saját magát (a fölötte lebegő szöveggel együtt) is kitörli a memóriából
                Destroy(gameObject);
            }
        }
    }

    // --- LATE UPDATE (Fizikai kényszerítések a mozgások után) ---
    private void LateUpdate()
    {
        // Biztonsági ellenőrzés: ha a tárgyat időközben megsemmisítették kívülről, nincs mit tenni
        if (grabbedObject == null) return;

        // Minden képkocka végén kényszerítjük, hogy az elkapott tárgy pontosan a megadott offset pozíción maradjon,
        // így semmilyen rángatózás vagy elcsúszás nem látszódhat a képernyőn.
        grabbedObject.localPosition = grabOffset;

        // Ha van fizikai teste, folyamatosan nullán tartjuk a sebességeit, hogy semmilyen külső fizikai erő ne mozdíthassa el
        if (grabbedRigidbody != null)
        {
            grabbedRigidbody.linearVelocity = Vector2.zero;
            grabbedRigidbody.angularVelocity = 0f;
        }
    }

    // --- A GÚNYOLÓDÓ SZÖVEG ELŐKÉSZÍTÉSE ---
    private void SetupTaunt()
    {
        // Ha az Inspectorban nem húztunk be kész szöveget, automatikusan létrehozunk egyet a karakter fölé
        if (tauntText == null)
        {
            // Létrehozunk egy új, üres GameObjectet, és ennek az objektumnak a gyermekévé tesszük (így együtt mozognak)
            GameObject textObject = new GameObject("TauntText");
            textObject.transform.SetParent(transform, false);

            // Ráteszünk egy TextMeshPro komponenst és beállítjuk a megjelenését
            tauntText = textObject.AddComponent<TextMeshPro>();
            tauntText.fontSize = tauntFontSize;
            tauntText.enableAutoSizing = false;
            tauntText.alignment = TextAlignmentOptions.Center;
            tauntText.rectTransform.sizeDelta = new Vector2(20f, 6f);

            // Beállítjuk, hogy a szöveg a karakter sprite-ja FÖLÖTT (előtt) rajzolódjon ki, ne mögötte
            MeshRenderer textRenderer = textObject.GetComponent<MeshRenderer>();
            if (textRenderer != null)
            {
                // Ugyanarra a rajzolási rétegre tesszük, mint a karaktert, majd 10-zel előrébb hozzuk
                if (selfRenderer != null)
                    textRenderer.sortingLayerID = selfRenderer.sortingLayerID;
                textRenderer.sortingOrder = (selfRenderer != null ? selfRenderer.sortingOrder : 0) + 10;
            }
        }

        // Kiegyenlítjük (ellensúlyozzuk) a karakter méretezését, hogy a szöveg ne nyúljon meg torzítva
        Vector3 lossy = transform.lossyScale;
        tauntText.transform.localScale = new Vector3(
            Mathf.Approximately(lossy.x, 0f) ? 1f : 1f / lossy.x,
            Mathf.Approximately(lossy.y, 0f) ? 1f : 1f / lossy.y,
            1f);
        tauntText.transform.localPosition = tauntOffset; // A szöveget a karakter fölé pozícionáljuk

        // Beállítjuk a szöveg tartalmát és színét
        tauntText.text = tauntMessage;
        tauntText.color = tauntColor;
        tauntBaseColor = tauntColor; // Elmentjük az eredeti színt, hogy később ehhez képest halványítsunk

        // Kezdetben elrejtjük – csak akkor jelenik meg, ha a játékos a közelébe ér
        tauntText.gameObject.SetActive(false);
    }

    // --- KÖZELSÉG ÉRZÉKELÉSE (a gúnyolódó szöveg megjelenítéséhez) ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ha már egyszer megjelent a szöveg, többször nem csináljuk meg
        if (tauntShown) return;
        // Csak a játékosra reagálunk, más objektumokra nem
        if (!other.CompareTag("Player")) return;

        tauntShown = true;

        // Megjelenítjük (bekapcsoljuk) a gúnyolódó szöveget
        if (tauntText != null)
            tauntText.gameObject.SetActive(true);
    }
}
