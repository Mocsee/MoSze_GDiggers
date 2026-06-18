using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // A jelenetek (pályák) újratöltéséhez szükséges névtér

public class PlayerMovement : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK (Inspector Headers & Fields) ---

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;          // Alapértelmezett sétatempó
    [SerializeField] private float sprintSpeed = 13f;       // Futási sebesség (Shift gombbal)
    [SerializeField] private float jumpForce = 18f;         // Az ugrás ereje (milyen magasra ugorjon)

    [Header("Gravity")]
    [SerializeField] private float normalGravity = 3f;       // Gravitáció mértéke, amikor felfelé ugrik a játékos
    [SerializeField] private float fallGravity = 6f;         // Erősebb gravitáció esés közben (hogy ne legyen "lebegős" az esés)

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers;         // Melyik rétegeket (Layer) tekintse a játék talajnak (pl. Ground, Platform)
    [SerializeField] private float groundCheckDistance = 0.08f; // Milyen mélyen nyúljon le a talajellenőrző doboz a karakter alá
    [SerializeField] private float groundCheckWidthMultiplier = 0.9f; // A talajellenőrző doboz szélessége a karakterhez képest (százalékban)

    [Header("Jump Assist")]
    [SerializeField] private float coyoteTime = 0.12f;      // "Coyote time": a peremről való lelépés után még ennyi másodpercig megengedett az ugrás

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;             // A fő kamera referenciája a képernyő széleinek kiszámításához
    [SerializeField] private float wrapPadding = 0.5f;       // Margó a képernyő szélén a vízszintes átforduláshoz (Pac-Man effekt)
    [SerializeField] private float wrapActivationDelay = 1f; // Mennyi idő után kapcsoljon be az átfordulás a pálya indulása után

    [Header("Death")]
    [SerializeField] private float deathOffsetBelowCamera = 5f; // Hány egységgel a kamera alja alatt számítson halottnak a karakter (ha leesik)
    [SerializeField] private float deathCheckDelay = 10f;       // Várakozási idő a pálya elején, mielőtt a halálzónát aktiválná

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer spriteRenderer;  // A karakter képét megjelenítő komponens
    [SerializeField] private Sprite idleSprite;              // Álló helyzeti kép
    [SerializeField] private Sprite[] movingSprites;         // Animációs fázisok képei a mozgáshoz (tömb)
    [SerializeField] private float framesPerSecond = 10f;    // Az animáció sebessége (hány képkocka váltson másodpercenként)

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private Rigidbody2D body;               // A karakter fizikai teste (mozgatáshoz, gravitációhoz)
    private BoxCollider2D boxCollider;       // A karakter ütközési doboza
    private float timeSinceSceneLoad;        // A pálya betöltése óta eltelt idő másodpercenként
    private Vector3 startingPosition;        // A játékos kezdőpozíciója a pályán
    private bool canMove = true;             // Mozoghat-e jelenleg a játékos (false pl. ha le van bénulva)
    private float coyoteCounter;             // Időzítő, ami a coyote time hátralévő idejét méri
    private float speedMultiplier = 1f;      // Sebességmódosító (pl. lassító zónában lecsökken)
    private Coroutine slowCoroutine;         // A lassításért felelős futó folyamat (Coroutine) referenciája
    private float animationTimer;            // Időzítő a képkockák váltásához
    private int animationIndex;              // Az éppen lejátszott mozgási képkocka indexe a tömbben

    // Kívülről olvasható tulajdonság, ami jelzi, hogy a játékos éppen sprintel-e
    public bool IsSprinting { get; private set; }

    // --- INITIALIZATION (Inicializálás) ---

    private void Awake()
    {
        // Összegyűjtjük a karakter objektumán található szükséges komponenseket
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Ha az Inspectorban nem lett beállítva a SpriteRenderer, megkeresi magán az objektumon
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        // Elmenti az induló pozíciót, hogy szükség esetén ide lehessen visszahelyezni
        startingPosition = transform.position;

        // Ha nincs kamera megadva, automatikusan megkeresi a játékban lévő fő kamerát
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Programozottan beállítja a talajnak minősülő rétegeket
        groundLayers = LayerMask.GetMask("Ground", "Platform");
    }

    private void Start()
    {
        // Biztosítja, hogy a játékos pontosan a kezdőpozíción indítson
        transform.position = startingPosition;
    }

    // --- GAME LOOP UPDATES (Minden képkockán lefutó logikák) ---

    private void Update()
    {
        // Növeli a jelenet betöltése óta eltelt időt
        timeSinceSceneLoad += Time.deltaTime;

        // Ellenőrzi, hogy a földön állunk-e
        bool grounded = IsGrounded();

        // Coyote time logika: ha a földön vagyunk, újraindítja a számlálót, különben folyamatosan csökkenti
        if (grounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        // Vízszintes input lekérése (-1: balra, 1: jobbra, 0: nincs gombnyomás)
        float xInput = Input.GetAxisRaw("Horizontal");

        // Ha a játékos szabadon mozoghat
        if (canMove)
        {
            // Sprintel, ha nyomja a Bal Shiftet ÉS ténylegesen van vízszintes irányú bemenet
            IsSprinting = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(xInput) > 0.01f;

            // Kiszámítja az alapsebességet attól függően, hogy sprintel vagy sétál
            float baseSpeed = IsSprinting ? sprintSpeed : moveSpeed;
            // Megszorozza az esetleges módosítókkal (pl. lassítás)
            float currentSpeed = baseSpeed * speedMultiplier;

            // Beállítja a Rigidbody sebességét: X tengelyen az input alapján, az Y tengelyen (esés/ugrás) meghagyja a fizikát
            body.linearVelocity = new Vector2(xInput * currentSpeed, body.linearVelocity.y);

            // Ugrás logika: Ha megnyomják a Szpészt ÉS a coyote időzítő még nem járt le (földön vagy a perem szélén van)
            if (Input.GetKeyDown(KeyCode.Space) && coyoteCounter > 0f)
            {
                // Felfelé irányú sebességet ad a karakternek
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
                // Nullázza a számlálót, hogy ne lehessen a levegőben duplán ugrani
                coyoteCounter = 0f;
            }
        }

        // Dinamikus gravitáció: Ha lefelé esik a karakter (Y sebesség < 0), nehezebb gravitációt kap, ha emelkedik, normált
        if (body.linearVelocity.y < 0f)
            body.gravityScale = fallGravity;
        else
            body.gravityScale = normalGravity;

        // Karakter képének (sprite) frissítése és animálása az irány alapján
        UpdateSprite(xInput);
    }

    // --- ANIMÁCIÓ ÉS GRAFIKA ---

    private void UpdateSprite(float xInput)
    {
        if (spriteRenderer == null) return; // Biztonsági ellenőrzés

        // Karakter megfordítása (Flip) a haladási iránynak megfelelően
        if (xInput > 0.01f)
            spriteRenderer.flipX = true;   // Jobbra néz
        else if (xInput < -0.01f)
            spriteRenderer.flipX = false;  // Balra néz

        // Akkor számít mozgásban lévőnek, ha van input ÉS a fizikai teste is valóban mozog vízszintesen
        bool isMoving = Mathf.Abs(xInput) > 0.01f && Mathf.Abs(body.linearVelocity.x) > 0.01f;

        // Ha mozog a karakter és vannak hozzárendelve animációs képkockák
        if (isMoving && movingSprites != null && movingSprites.Length > 0)
        {
            animationTimer += Time.deltaTime; // Idő mérése
            // Kiszámítja egy képkocka hosszát a megadott FPS-ből (pl. 10 FPS esetén 0.1 másodperc)
            float frameDuration = framesPerSecond > 0f ? 1f / framesPerSecond : 0.1f;
            
            // Ha eltelt egy képkockányi idő, váltunk a következő képre
            if (animationTimer >= frameDuration)
            {
                animationTimer -= frameDuration;
                // Lépteti az indexet, a '%' (maradékos osztás) biztosítja, hogy ha a végére ér, elölről kezdje (Loop)
                animationIndex = (animationIndex + 1) % movingSprites.Length;
            }
            // Beállítja az aktuális animációs fázis képét
            spriteRenderer.sprite = movingSprites[animationIndex];
        }
        else // Ha a karakter egy helyben áll
        {
            animationTimer = 0f;
            animationIndex = 0;
            // Visszaállítja az alapértelmezett álló képet
            if (idleSprite != null)
                spriteRenderer.sprite = idleSprite;
        }
    }

    // LateUpdate a fizikai mozgások kiszámítása UTÁN fut le, tökéletes kamerához és pozíció ellenőrzésekhez
    private void LateUpdate()
    {
        WrapHorizontally();        // Képernyő szélein való átfordulás kezelése
        CheckIfFellBelowCamera();  // Annak ellenőrzése, hogy a játékos leesett-e a szakadékba
    }

    // --- FIZIKAI ÉS KÖRNYEZETI ELLENŐRZÉSEK ---

    // Megvizsgálja, hogy a játékos a talajon áll-e egy lefelé lőtt virtuális doboz segítségével (BoxCast)
    public bool IsGrounded()
    {
        if (boxCollider == null) return false;

        Bounds bounds = boxCollider.bounds;

        // Kiszámolja a doboz szélességét a szorzó alapján (kissé keskenyebb, mint a karakter, hogy ne akadjon el a sarkokban)
        float castWidth = bounds.size.x * groundCheckWidthMultiplier;
        Vector2 boxCastSize = new Vector2(castWidth, bounds.size.y);
        Vector2 boxCastOrigin = bounds.center;

        // Egy láthatalan dobozt vetít ki lefelé a fizikai térben
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCastOrigin,        // Kiindulópont (a karakter közepe)
            boxCastSize,          // A vizsgálandó doboz mérete
            0f,                   // Elforgatási szög
            Vector2.down,         // Irány (lefelé)
            groundCheckDistance,  // Milyen messzire nyúljon le
            groundLayers          // Melyik rétegeket figyelje
        );

        // Ha a vetített doboz eltalált valamit (nem null), akkor a játékos a földön van
        return hit.collider != null;
    }

    // Pac-Man stílusú képernyő-átfordulás: ha kimegy a karakter jobbra, bejön balról
    private void WrapHorizontally()
    {
        if (mainCamera == null) return;
        if (timeSinceSceneLoad < wrapActivationDelay) return; // Nem engedi aktiválódni rögtön a pálya elején

        // Kamera méreteinek és pozíciójának lekérése
        float camHeight = mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect; // Szélesség kiszámítása a képernyőarányból
        float camX = mainCamera.transform.position.x;

        // Megállapítja a játékos szélességének felét az ütközőből
        float halfPlayerWidth = 0.5f;
        if (boxCollider != null)
            halfPlayerWidth = boxCollider.bounds.extents.x;

        // Kiszámítja a képernyő bal és jobb oldali abszolút határvonalát
        float leftBound = camX - camWidth - halfPlayerWidth;
        float rightBound = camX + camWidth + halfPlayerWidth;

        Vector3 pos = transform.position;

        // Ha túlhaladt a jobb szélen: átrakja a bal szélre
        if (pos.x > rightBound + wrapPadding)
        {
            pos.x = leftBound + wrapPadding;
            transform.position = pos;
        }
        // Ha túlhaladt a bal szélen: átrakja a jobb szélre
        else if (pos.x < leftBound - wrapPadding)
        {
            pos.x = rightBound - wrapPadding;
            transform.position = pos;
        }
    }

    // Ellenőrzi, hogy a játékos bezuhant-e a kamera alá (szakadékba esés)
    private void CheckIfFellBelowCamera()
    {
        if (mainCamera == null) return;
        if (timeSinceSceneLoad < deathCheckDelay) return; // Biztonsági idő a pálya kezdetén

        // Kiszámítja a kamera legalsó látható pontját
        float cameraBottom = mainCamera.transform.position.y - mainCamera.orthographicSize;
        // Meghatározza a halálos magasságot (Y koordinátát)
        float deathY = cameraBottom - deathOffsetBelowCamera;

        // Ha a játékos ez alá a vonal alá esik, a jelenlegi pálya újraindul
        if (transform.position.y < deathY)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // --- KÍVÜLRŐL MEGHÍVHATÓ FUNKCIÓK (Effektek, Sebzések) ---

    // Külső szkriptek meghívhatják ezt, hogy átmenetileg letiltsák a játékos mozgását
    public void DisableMovementTemporarily(float duration)
    {
        StartCoroutine(DisableMovementCoroutine(duration));
    }

    // Időzített folyamat a mozgás letiltására
    private IEnumerator DisableMovementCoroutine(float duration)
    {
        canMove = false; // Mozgás kikapcsolása
        yield return new WaitForSeconds(duration); // Várakozás a megadott ideig
        canMove = true;  // Mozgás visszakapcsolása
    }

    // Lassító effekt alkalmazása (pl. ha a SlowZone-ba lép a karakter)
    public void ApplySlow(float slowPercent, float duration)
    {
        // Ha már fut egy lassítás, azt leállítja, hogy az új lépjen életbe
        if (slowCoroutine != null)
            StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(ApplySlowCoroutine(slowPercent, duration));
    }

    // Időzített folyamat a lassítás lefolytatására
    private IEnumerator ApplySlowCoroutine(float slowPercent, float duration)
    {
        // Kiszámítja az új sebségszorzót (biztosítja, hogy ne lassuljon 0 alá, minimum 0.05f megmaradjon)
        speedMultiplier = Mathf.Clamp(1f - slowPercent, 0.05f, 1f);
        yield return new WaitForSeconds(duration); // Várakozás, amíg tart a hatás
        speedMultiplier = 1f; // Sebesség visszaállítása normálra
        slowCoroutine = null;  // Folyamat referenciájának ürítése
    }

    // --- EDITOR RENDERING (Segédvonalak rajzolása a fejlesztőknek) ---

    // Kirajzolja a talajellenőrző dobozt a Unity Scene nézetében, ha a játékos ki van jelölve
    private void OnDrawGizmosSelected()
    {
        BoxCollider2D bc = GetComponent<BoxCollider2D>();
        if (bc != null)
        {
            Gizmos.color = Color.red; // Piros színű vonalakkal fog rajzolni

            Bounds bounds = bc.bounds;
            float castWidth = bounds.size.x * groundCheckWidthMultiplier;
            Vector3 boxSize = new Vector3(castWidth, bounds.size.y, 0f);
            // Kiszámítja, pontosan hová fog esni a doboz a karakter alatt játék közben
            Vector3 boxCenter = bounds.center + Vector3.down * groundCheckDistance;

            // Drótvázas kocka kirajzolása a tesztelés megkönnyítésére
            Gizmos.DrawWireCube(boxCenter, boxSize);
        }
    }
}
