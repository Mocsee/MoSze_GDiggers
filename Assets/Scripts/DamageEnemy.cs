using UnityEngine;

public class DamageEnemy : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;         // Az ellenség haladási sebessége üldözés közben
    [SerializeField] private float detectionRange = 10f;    // Az a hatósugár, amin belül észleli és üldözni kezdi a játékost

    [Header("Projectile Throwing")]
    [SerializeField] private GameObject projectilePrefab;   // A kilőni kívánt lövedék előregyártott objektuma (Prefab)
    [SerializeField] private Transform firePoint;           // A pont az ellenségen, ahonnan a lövedék elindul
    [SerializeField] private float throwCooldown = 2f;       // Két lövés között eltelt kötelező várakozási idő
    [SerializeField] private float projectileSpeed = 6f;    // A kilőtt lövedék repülési sebessége

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer; // A grafika megjelenítéséért felelős komponens

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;              // Álló kép, ha nincs senki a közelben
    [SerializeField] private Sprite[] movingSprites;         // Mozgás közbeni animációs fázisok képei
    [SerializeField] private float framesPerSecond = 10f;    // Az animáció sebessége (FPS)

    [Header("Pushback")]
    [SerializeField] private float pushStrength = 4f;       // A visszapattanás ereje, ha fizikailag hozzáér a játékoshoz
    [SerializeField] private float pushDamping = 8f;        // Milyen gyorsan fékeződjön le a visszapattanás után (súrlódás)
    [SerializeField] private float pushCooldown = 0.15f;    // Minimális szünet az újabb visszapattanások aktiválódása között

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private Transform player;                     // Referencia a játékos pozíciójához (Transform)
    private float throwTimer;                     // Időzítő a lövések közötti cooldown mérésére
    private bool isDead;                          // Halott-e már az ellenség
    private float pushCooldownTimer;              // Időzítő a visszapattanási szünet kezelésére
    private Vector3 firePointOriginalLocalPosition; // Elmenti a lövési pont eredeti helyzetét a tükrözéshez
    private Vector2 pushVelocity;                 // Az aktuális visszalökési sebességvektor
    private bool isMoving;                        // Éppen mozgásban van-e az ellenség
    private float animationTimer;                 // Időzítő a képkockák váltásához
    private int animationIndex;                   // Az éppen aktív animációs képkocka indexe

    // --- INITIALIZATION (Inicializálás) ---
    private void Start()
    {
        // Megkeresi a játékost a "Player" címke (Tag) alapján a jelenetben
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;

        // Beállítja az időzítőt, hogy ne lőjön azonnal a pálya betöltésekor
        throwTimer = throwCooldown;

        // Elmenti a lövési pont relatív (helyi) pozícióját, hogy irányváltáskor tükrözni lehessen
        if (firePoint != null)
            firePointOriginalLocalPosition = firePoint.localPosition;
    }

    // --- FŐ LOGIKAI KÖR (Minden képkockán) ---
    private void Update()
    {
        if (isDead) return;        // Ha halott, azonnal leáll a működés
        if (player == null) return; // Ha nincs játékos, nincs kit támadni

        // Csökkenti a visszalökési korlátozás időzítőjét az idő múlásával
        if (pushCooldownTimer > 0f)
            pushCooldownTimer -= Time.deltaTime;

        UpdatePushback();  // Visszalökési sebesség folyamatos lefékezése
        HandleBehaviour(); // Távolságmérés, üldözés és lövési logika
        UpdateSprite();    // Animációk frissítése
    }

    // --- VISZELKEDÉSI LOGIKA ---
    private void HandleBehaviour()
    {
        // Kiszámítja a játékos és az ellenség közötti távolságot
        float distance = Vector2.Distance(transform.position, player.position);

        // Ha a játékos az észlelési hatósugáron belül van
        if (distance <= detectionRange)
        {
            MoveTowardsPlayer();         // Elindul a játékos felé
            HandleProjectileThrowing();  // Kezeli a lövedékek indítását
            UpdateFacingDirection();     // Grafika és fegyvercső megfelelő irányba forgatása
            isMoving = true;             // Beállítja a mozgási állapotot az animációhoz
        }
        else
        {
            isMoving = false;            // Ha messze van, megáll és leállítja a mozgási animációt
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = player.position;

        // Kiszámítja a tiszta üldözési irányt és a normál lépést erre a képkockára
        Vector2 chaseDirection = (targetPosition - currentPosition).normalized;
        Vector2 chaseMovement = chaseDirection * moveSpeed * Time.deltaTime;

        // A végső elmozdulás a normál üldözési mozgás ÉS a visszalökési sebesség (pushVelocity) összege.
        // Ez biztosítja, hogy ütközés után hátráljon az ellenség, hiába akar előre haladni.
        Vector2 finalMovement = chaseMovement + (pushVelocity * Time.deltaTime);
        transform.position += (Vector3)finalMovement;
    }

    // Fokozatosan lecsillapítja (lefékezi) a visszalökési erőt nullára (Vector2.zero) a megadott damping sebességgel
    private void UpdatePushback()
    {
        pushVelocity = Vector2.Lerp(pushVelocity, Vector2.zero, pushDamping * Time.deltaTime);
    }

    // --- GRAFIKA ÉS FEGYVERCSŐ TÜKRÖZÉSE ---
    private void UpdateFacingDirection()
    {
        if (spriteRenderer == null) return;

        // Ha a játékos az ellenségtől BALRA van
        if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = false; // Alapértelmezett, balra néző sprite állapot

            // A lövési pontot (firePoint) is átrakja az ellenség BAL oldalára (negatív X)
            if (firePoint != null)
                firePoint.localPosition = new Vector3(-Mathf.Abs(firePointOriginalLocalPosition.x), firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
        }
        // Ha a játékos az ellenségtől JOBBRA van
        else if (player.position.x > transform.position.x)
        {
            spriteRenderer.flipX = true;  // Tükrözi a grafikát jobbra

            // A lövési pontot (firePoint) is átrakja az ellenség JOBB oldalára (pozitív X)
            if (firePoint != null)
                firePoint.localPosition = new Vector3(Mathf.Abs(firePointOriginalLocalPosition.x), firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
        }
    }

    // --- MOZGÁSI ANIMÁCIÓ KEZELÉSE ---
    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        // Ha az ellenség mozog és vannak hozzárendelve képek
        if (isMoving && movingSprites != null && movingSprites.Length > 0)
        {
            animationTimer += Time.deltaTime;
            float frameDuration = framesPerSecond > 0f ? 1f / framesPerSecond : 0.1f;
            
            if (animationTimer >= frameDuration)
            {
                animationTimer -= frameDuration;
                animationIndex = (animationIndex + 1) % movingSprites.Length; // Újrainduló számláló (Loop)
            }
            spriteRenderer.sprite = movingSprites[animationIndex];
        }
        else // Ha egy helyben áll, visszaáll az alapértelmezett kép
        {
            animationTimer = 0f;
            animationIndex = 0;
            if (idleSprite != null)
                spriteRenderer.sprite = idleSprite;
        }
    }

    // --- LÖVÉSI IDŐZÍTŐ ---
    private void HandleProjectileThrowing()
    {
        throwTimer -= Time.deltaTime; // Csökkenti a visszaszámlálót

        // Ha letelt a cooldown
        if (throwTimer <= 0f)
        {
            ThrowProjectile();        // Kilövi a lövedéket
            throwTimer = throwCooldown; // Újraindítja a visszaszámlálót
        }
    }

    private void ThrowProjectile()
    {
        // Biztonsági ellenőrzés: ha hiányzik a lövedék, a pont vagy a játékos, nem csinál semmit
        if (projectilePrefab == null || firePoint == null || player == null) return;

        // Létrehozza a lövedéket a firePoint aktuális világpozíciójában, forgatás nélkül
        GameObject projectileObject = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        // Lekéri a lövedéken található önálló mozgató szkriptet (DamageProjectile)
        DamageProjectile projectile = projectileObject.GetComponent<DamageProjectile>();

        if (projectile != null)
        {
            // Kiszámítja a célirányt a fegyvertől a játékos felé, majd normalizálja azt
            Vector2 direction = (player.position - firePoint.position).normalized;
            // Inicializálja a lövedéket, átadva az irányt és a sebességet
            projectile.Initialize(direction, projectileSpeed);
        }
    }

    // --- FOLYAMATOS ÜTKÖZÉS (Visszalökés) ---
    // Akkor fut le minden képkockán, amíg az ellenség teste fizikailag hozzáér a játékoshoz
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (pushCooldownTimer > 0f) return; // Ha nemrég lökődött vissza, még nem lökődhet újra
        if (!collision.gameObject.CompareTag("Player")) return; // Csak a játékosra reagál

        // Kiszámítja a játékostól elfelé mutató irányvektort
        Vector2 awayFromPlayer = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;

        // Ha hajszálpontosan egy pozícióban állnának, alapértelmezetten jobbra lökődik el
        if (awayFromPlayer == Vector2.zero)
            awayFromPlayer = Vector2.right;

        // Meglöki az ellenséget a játékostól elfelé (ezt a pushVelocity-t alkalmazza a MoveTowardsPlayer)
        pushVelocity = awayFromPlayer * pushStrength;
        pushCooldownTimer = pushCooldown; // Elindítja a cooldown időzítőt
    }

    // --- HALÁL ---
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Destroy(gameObject); // Megsemmisíti ezt az ellenséget
    }
}
