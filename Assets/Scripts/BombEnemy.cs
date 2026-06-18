using UnityEngine;

public class BombEnemy : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;         // Az ellenség mozgási sebessége üldözés közben
    [SerializeField] private float detectionRange = 10f;    // Az a hatósugár, amin belül észleli és támadja a játékost

    [Header("Bomb Throwing")]
    [SerializeField] private GameObject bombPrefab;         // A dobni kívánt bomba előregyártott objektuma (Prefab)
    [SerializeField] private Transform firePoint;           // A pont (üres objektum) az ellenségen, ahonnan a bomba elindul
    [SerializeField] private float bombCooldown = 2f;       // Két bombadobás között eltelt kötelező várakozási idő
    [SerializeField] private float bombSpeed = 6f;          // A feldobott bomba repülési sebessége

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer; // A vizuális megjelenítésért felelős komponens

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;              // Álló helyzeti kép, ha a játékos nincs a közelben
    [SerializeField] private Sprite[] movingSprites;         // Mozgási animáció képkockáinak tömbje
    [SerializeField] private float framesPerSecond = 10f;    // Az animáció lejátszási sebessége (FPS)

    [Header("Pushback")]
    [SerializeField] private float pushStrength = 4f;       // A visszapattanás ereje, ha a játékoshoz ér
    [SerializeField] private float pushDamping = 8f;        // Milyen gyorsan fékeződjön le a visszapattanás után
    [SerializeField] private float pushCooldown = 0.15f;    // Minimális szünet az újabb visszapattanások között

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private Transform player;                     // Referencia a játékos pozíciójához
    private float bombTimer;                      // Időzítő a bombadobás visszaszámlálásához
    private bool isDead;                          // Halott-e már az ellenség
    private float pushCooldownTimer;              // Időzítő a visszapattanási cooldown mérésére
    private Vector3 firePointOriginalLocalPosition; // Elmenti a dobópont eredeti helyzetét a tükrözéshez
    private Vector2 pushVelocity;                 // Az aktuális visszalökési sebességvektor
    private bool isMoving;                        // Éppen mozgásban van-e az ellenség
    private float animationTimer;                 // Időzítő az animációs képkockák váltásához
    private int animationIndex;                   // Az éppen aktív mozgási képkocka indexe

    // --- INITIALIZATION (Inicializálás) ---
    private void Start()
    {
        // Megkeresi a játékost a "Player" címke (Tag) alapján
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            // Egyedi funkció: kikapcsolja a fizikai ütközést az ellenség és a játékos teste között,
            // hogy ne akadjanak össze mereven a fizikai dobozaik (Colliderek)
            IgnorePlayerCollisions(playerObject);
        }

        // Úgy indítjuk az időzítőt, hogy ne tudjon azonnal bombát dobni a betöltés pillanatában
        bombTimer = bombCooldown;

        // Elmenti a bombaindítási pont relatív (helyi) pozícióját
        if (firePoint != null)
            firePointOriginalLocalPosition = firePoint.localPosition;
    }

    // Végigmegy az összes ütközőn és letiltja a merev fizikai blokkolást az ellenség és játékos között
    private void IgnorePlayerCollisions(GameObject playerObject)
    {
        Collider2D[] enemyColliders = GetComponentsInChildren<Collider2D>();
        Collider2D[] playerColliders = playerObject.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            if (enemyCollider.isTrigger) continue; // A trigger típusú (érzékelő) ütközőket békén hagyja
            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider.isTrigger) continue;
                // Megparancsolja a Unity fizikai motorjának, hogy ez a két collider csússzon át egymáson (true)
                Physics2D.IgnoreCollision(enemyCollider, playerCollider, true);
            }
        }
    }

    // --- FŐ LOGIKAI KÖR (Minden képkockán) ---
    private void Update()
    {
        if (isDead) return;        // Ha halott, megáll a működés
        if (player == null) return; // Ha nincs játékos, nincs mit tenni

        // Csökkenti a visszalökési korlátozás időzítőjét
        if (pushCooldownTimer > 0f)
            pushCooldownTimer -= Time.deltaTime;

        UpdatePushback();  // Visszalökési erő csillapítása/fékezése
        HandleBehaviour(); // Játékos távolságának ellenőrzése, mozgás, dobás
        UpdateSprite();    // Animációk frissítése
    }

    // --- VISZELKEDÉS ÉS MOZGÁS ---
    private void HandleBehaviour()
    {
        // Kiszámítja a játékos és az ellenség közötti távolságot
        float distance = Vector2.Distance(transform.position, player.position);

        // Ha a hatósugáron belül van a játékos
        if (distance <= detectionRange)
        {
            MoveTowardsPlayer();      // Elindul a játékos felé
            HandleBombThrowing();     // Kezeli a bombák dobálását
            UpdateFacingDirection();  // Fordítsa a grafikát és a dobópontot a megfelelő irányba
            isMoving = true;          // Beállítja az animációhoz a mozgási flag-et
        }
        else
        {
            isMoving = false;         // Ha messze van, megáll és leállítja a mozgási animációt
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = player.position;

        // Kiszámítja a tiszta üldözési irányt és a normál lépési távolságot erre a képkockára
        Vector2 chaseDirection = (targetPosition - currentPosition).normalized;
        Vector2 chaseMovement = chaseDirection * moveSpeed * Time.deltaTime;

        // --- KOMPLEX MOZGÁSI TRÜKK ---
        // A végső mozgás az üldözési elmozdulás ÉS a visszalökési sebesség (pushVelocity) összege.
        // Ha az ellenség épp visszapattan a játékosról, a pushVelocity nagyobb lesz, mint a chaseMovement,
        // így az ellenség fizikailag hátrálni fog, hiába próbálna előre menni.
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
            spriteRenderer.flipX = false; // Alapértelmezett (balra néző) sprite állapot

            // Ha van fegyver/dobó cső (firePoint), azt is átrakja az ellenség BAL oldalára (negatív X)
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

    // --- ANIMÁCIÓ KEZELÉSE ---
    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        // Ha mozgásban van és vannak megadva mozgási képkockák
        if (isMoving && movingSprites != null && movingSprites.Length > 0)
        {
            animationTimer += Time.deltaTime;
            float frameDuration = framesPerSecond > 0f ? 1f / framesPerSecond : 0.1f;
            
            if (animationTimer >= frameDuration)
            {
                animationTimer -= frameDuration;
                animationIndex = (animationIndex + 1) % movingSprites.Length; // Loop típusú léptetés
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

    // --- BOMBADOBOZ LOGIKA ---
    private void HandleBombThrowing()
    {
        bombTimer -= Time.deltaTime; // Csökkenti a visszaszámlálót

        // Ha letelt a cooldown idő
        if (bombTimer <= 0f)
        {
            ThrowBomb();             // Eldobja a bombát
            bombTimer = bombCooldown; // Újraindítja a visszaszámlálót
        }
    }

    private void ThrowBomb()
    {
        // Biztonsági ellenőrzés: ha bármelyik szükséges elem hiányzik, megszakítja a dobást
        if (bombPrefab == null || firePoint == null || player == null) return;

        // Létrehozza (Spawolja) a bomba Prefabot a firePoint aktuális pozíciójában, forgatás nélkül
        GameObject bombObject = Instantiate(bombPrefab, firePoint.position, Quaternion.identity);
        
        // Megpróbálja lekérni a bombán elhelyezett lövedék-szkriptet (BombProjectile)
        BombProjectile bomb = bombObject.GetComponent<BombProjectile>();

        if (bomb != null)
        {
            // Kiszámítja a dobás irányát a fegyvercsőtől a játékos felé, majd normalizálja (hossza 1 lesz)
            Vector2 direction = (player.position - firePoint.position).normalized;
            // Átadja az iránymutatást és a sebességet a bombának, ami ezután önállóan repül tovább
            bomb.Initialize(direction, bombSpeed);
        }
    }

    // --- FOLYAMATOS JÁTÉKOSHOZ ÉRÉS (Visszalökés) ---
    // Akkor fut le minden képkockán, amíg az ellenség fizikailag hozzáér (súrlódik) a játékoshoz
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (pushCooldownTimer > 0f) return; // Ha nemrég lökődött vissza, nem aktiválódik újra azonnal
        if (!collision.gameObject.CompareTag("Player")) return; // Csak a játékos érintésére reagál

        // Kiszámítja a játékostól elfelé mutató irányt
        Vector2 awayFromPlayer = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;

        // Ha a pozícióik hajszálpontosan megegyeznének (0 irány), alapértelmezetten jobbra lökődik
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
