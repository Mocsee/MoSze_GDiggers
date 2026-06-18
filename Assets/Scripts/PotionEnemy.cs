using UnityEngine;

public class PotionEnemy : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;         // Az ellenség mozgási sebessége üldözés közben
    [SerializeField] private float detectionRange = 10f;    // Az észlelési hatósugár, amin belül üldözni kezdi a játékost

    [Header("Potion Throwing")]
    [SerializeField] private GameObject potionPrefab;       // A bájital lövedék előregyártott objektuma (Prefab)
    [SerializeField] private Transform firePoint;           // A pont az ellenségen, ahonnan a bájital elindul
    [SerializeField] private float potionCooldown = 2.5f;   // Két dobás között eltelt kötelező várakozási idő
    [SerializeField] private float potionSpeed = 5f;        // A bájital repülési sebessége

    [Header("Slow Effect")]
    // Ezeket az értékeket az ellenség át fogja adni a bájitalnak a dobáskor
    [SerializeField] private float slowPercent = 0.5f;      // Lassítás mértéke (pl. 0.5 = 50%-os lassítás)
    [SerializeField] private float slowDuration = 2f;       // Meddig tartson a lassító hatás (másodpercben)
    [SerializeField] private float slowZoneRadius = 1.5f;   // A becsapódás után létrejövő lassító zóna sugara

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer; // A grafika megjelenítéséért felelős komponens

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;              // Álló kép, ha a játékos nincs a közelben
    [SerializeField] private Sprite[] movingSprites;         // Mozgás közbeni animáció képkockái
    [SerializeField] private float framesPerSecond = 10f;    // Az animáció lejátszási sebessége (FPS)

    [Header("Pushback")]
    [SerializeField] private float pushStrength = 4f;       // A visszapattanás ereje, ha fizikailag hozzáér a játékoshoz
    [SerializeField] private float pushDamping = 8f;        // Milyen gyorsan fékeződjön le a visszapattanás után
    [SerializeField] private float pushCooldown = 0.15f;    // Minimális szünet az újabb visszapattanások között

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private Transform player;                     // Referencia a játékos pozíciójához
    private float potionTimer;                    // Időzítő a dobási cooldown mérésére
    private bool isDead;                          // Halott-e már az ellenség
    private Vector3 firePointOriginalLocalPosition; // Elmenti a dobópont eredeti helyzetét a tükrözéshez

    private Vector2 pushVelocity;                 // Az aktuális visszalökési sebességvektor
    private float pushCooldownTimer;              // Időzítő a visszapattanási szünet kezelésére
    private bool isMoving;                        // Éppen mozgásban van-e az ellenség
    private float animationTimer;                 // Időzítő a képkockák váltásához
    private int animationIndex;                   // Az éppen aktív animációs képkocka indexe

    // --- INITIALIZATION (Inicializálás) ---
    private void Start()
    {
        // Megkeresi a játékost a "Player" tag alapján
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;

        // Úgy indítja az időzítőt, hogy ne dobjon azonnal potit a játék elindulásakor
        potionTimer = potionCooldown;

        // Elmenti a fegyvercső/dobópont relatív pozícióját az irányváltásokhoz
        if (firePoint != null)
            firePointOriginalLocalPosition = firePoint.localPosition;
    }

    // --- FŐ LOGIKAI KÖR (Minden képkockán) ---
    private void Update()
    {
        if (isDead) return;        // Ha halott, megáll a működés
        if (player == null) return; // Ha nincs játékos, nincs kit támadni

        // Csökkenti a visszalökési korlátozás időzítőjét
        if (pushCooldownTimer > 0f)
            pushCooldownTimer -= Time.deltaTime;

        UpdatePushback();  // Visszalökési sebesség folyamatos csillapítása
        HandleBehaviour(); // Távolságellenőrzés, üldözés és dobási logika
        UpdateSprite();    // Animáció frissítése
    }

    // --- VISZELKEDÉSI LOGIKA ---
    private void HandleBehaviour()
    {
        // Kiszámítja a távolságot az ellenség és a játékos között
        float distance = Vector2.Distance(transform.position, player.position);

        // Ha a játékos az észlelési hatósugáron belül van
        if (distance <= detectionRange)
        {
            MoveTowardsPlayer();       // Elindul a játékos felé
            HandlePotionThrowing();    // Kezeli a bájital dobálását
            UpdateFacingDirection();   // Grafikát és dobópontot a megfelelő irányba forgatja
            isMoving = true;           // Animációhoz beállítja a mozgási flag-et
        }
        else
        {
            isMoving = false;          // Ha messze van, megáll
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = player.position;

        // Kiszámítja az üldözési irányt és a normál lépést erre a képkockára
        Vector2 chaseDirection = (targetPosition - currentPosition).normalized;
        Vector2 chaseMovement = chaseDirection * moveSpeed * Time.deltaTime;

        // A végső elmozdulás az üldözési sebesség és a visszalökési sebesség (pushVelocity) összege
        Vector2 finalMovement = chaseMovement + (pushVelocity * Time.deltaTime);
        transform.position += (Vector3)finalMovement;
    }

    // Fokozatosan lefékezi a visszalökési erőt nullára (Vector2.zero) a pushDamping sebességgel
    private void UpdatePushback()
    {
        pushVelocity = Vector2.Lerp(pushVelocity, Vector2.zero, pushDamping * Time.deltaTime);
    }

    // --- IRÁNYBA FORDULÁS (Sprite & FirePoint tükrözés) ---
    private void UpdateFacingDirection()
    {
        if (spriteRenderer == null) return;

        // Ha a játékos az ellenségtől BALRA helyezkedik el
        if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = false; // Alapértelmezett (balra néző) állapot

            // A dobópontot (firePoint) is átrakja az ellenség BAL oldalára (negatív X)
            if (firePoint != null)
                firePoint.localPosition = new Vector3(-Mathf.Abs(firePointOriginalLocalPosition.x), firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
        }
        // Ha a játékos az ellenségtől JOBBRA helyezkedik el
        else if (player.position.x > transform.position.x)
        {
            spriteRenderer.flipX = true;  // Tükrözi a grafikát jobbra

            // A dobópontot (firePoint) is átrakja az ellenség JOBB oldalára (pozitív X)
            if (firePoint != null)
                firePoint.localPosition = new Vector3(Mathf.Abs(firePointOriginalLocalPosition.x), firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
        }
    }

    // --- MOZGÁSI ANIMÁCIÓ KEZELÉSE ---
    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        // Ha mozgásban van és vannak hozzárendelve képek
        if (isMoving && movingSprites != null && movingSprites.Length > 0)
        {
            animationTimer += Time.deltaTime;
            float frameDuration = framesPerSecond > 0f ? 1f / framesPerSecond : 0.1f;
            
            if (animationTimer >= frameDuration)
            {
                animationTimer -= frameDuration;
                animationIndex = (animationIndex + 1) % movingSprites.Length; // Loop típusú léptetés (modulo)
            }
            spriteRenderer.sprite = movingSprites[animationIndex];
        }
        else // Álló helyzeti grafika visszaállítása
        {
            animationTimer = 0f;
            animationIndex = 0;
            if (idleSprite != null)
                spriteRenderer.sprite = idleSprite;
        }
    }

    // --- DOBÁSI IDŐZÍTŐ ---
    private void HandlePotionThrowing()
    {
        potionTimer -= Time.deltaTime; // Csökkenti a visszaszámlálót

        // Ha letelt a cooldown idő
        if (potionTimer <= 0f)
        {
            ThrowPotion();             // Eldobja a bájitalt
            potionTimer = potionCooldown; // Újraindítja a visszaszámlálót
        }
    }

    private void ThrowPotion()
    {
        // Biztonsági ellenőrzés
        if (potionPrefab == null || firePoint == null || player == null) return;

        // Létrehozza a bájital Prefabot a firePoint pozíciójában, forgatás nélkül
        GameObject potionObject = Instantiate(potionPrefab, firePoint.position, Quaternion.identity);
        
        // Lekéri a bájital saját lövedék-szkriptjét (PotionProjectile)
        PotionProjectile potion = potionObject.GetComponent<PotionProjectile>();

        if (potion != null)
        {
            // Kiszámítja a dobás irányát a firePointtól a játékos felé, majd normalizálja azt
            Vector2 direction = (player.position - firePoint.position).normalized;
            
            // --- EGYEDI INICIALIZÁLÁS ---
            // Átadja a bájitalnak a repülési adatokat ÉS az Inspectorban beállított egyedi lassítási tulajdonságokat is,
            // így a lövedék tudni fogja, mekkora és milyen erős lassító zónát kell generálnia a becsapódáskor.
            potion.Initialize(direction, potionSpeed, slowPercent, slowDuration, slowZoneRadius);
        }
    }

    // --- FOLYAMATOS ÜTKÖZÉS (Visszalökés) ---
    // Akkor fut le minden képkockán, amíg az ellenség fizikailag hozzáér a játékoshoz
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (pushCooldownTimer > 0f) return; // Ha nemrég lökődött vissza, nem aktiválódik újra azonnal
        if (!collision.gameObject.CompareTag("Player")) return; // Csak a játékosra reagál

        // Kiszámítja a játékostól elfelé mutató irányt
        Vector2 awayFromPlayer = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;

        if (awayFromPlayer == Vector2.zero)
            awayFromPlayer = Vector2.right;

        // Beállítja a pushVelocity értékét, ami a MoveTowardsPlayer-ben eltolja az ellenséget hátrafelé
        pushVelocity = awayFromPlayer * pushStrength;
        pushCooldownTimer = pushCooldown; // Elindítja a mini cooldown-t
    }

    // --- HALÁL ---
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Destroy(gameObject); // Megsemmisíti az ellenséget
    }
}
