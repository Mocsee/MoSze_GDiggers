using UnityEngine;

public class PotionProjectile : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    // A repülő bájital maximális élettartama. Ha nem talál el semmit, ennyi idő után magától eltűnik.
    [SerializeField] private float lifetime = 5f;
    // A becsapódás pillanatában legenerálódó lassító zóna előregyártott objektuma (Prefab)
    [SerializeField] private GameObject slowZonePrefab;

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private Vector2 moveDirection; // A repülés iránya
    private float moveSpeed;       // A repülés sebessége
    private bool initialized;      // Be lett-e már lőve a bájital (megkapta-e az adatokat)
    private bool hasExploded;      // Felrobbant-e már a bájital (biztonsági flag a többszörös robbanás ellen)

    // Az ellenségtől átvett lassítási tulajdonságok, amiket majd a lassító zónának kell továbbadni
    private float slowPercent;
    private float slowDuration;
    private float slowZoneRadius;

    // --- MEGHÍVHATÓ INICIALIZÁLÓ FÜGGVÉNY ---
    // Ezt a PotionEnemy hívja meg a dobás pillanatában, hogy átadja az összes fontos paramétert
    public void Initialize(Vector2 direction, float speed, float newSlowPercent, float newSlowDuration, float newSlowZoneRadius)
    {
        moveDirection = direction.normalized; // Irányvektor hosszak kényszerítése 1-re
        moveSpeed = speed;                   // Sebesség beállítása
        slowPercent = newSlowPercent;        // Lassítás mértéke
        slowDuration = newSlowDuration;      // Lassítás időtartama
        slowZoneRadius = newSlowZoneRadius;  // Zóna mérete
        initialized = true;                  // Engedélyezi a mozgást és az élettartamot
    }

    // --- INDULÁSI LOGIKA ---
    private void Start()
    {
        // Ha sikeresen el lett indítva a lövedék, beidőzítjük az automatikus törlését (lifetime)
        if (initialized)
            Destroy(gameObject, lifetime);
    }

    // --- FŐ MOZGÁSI KÖR (Minden képkockán) ---
    private void Update()
    {
        // Ha nincs inicializálva, vagy MÁR FELROBBANT, akkor nem mozdul el sehova
        if (!initialized || hasExploded) return;

        // Egyenes vonalban mozgatja a bájitalt a megadott irányba és sebességgel
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    // --- ÜTKÖZÉSEK FIGYELÉSE ---
    // 1. Eset: Ha a lövedék egy Trigger típusú (átjárható) ütközőhöz ér
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryExplode(other.gameObject);
    }

    // 2. Eset: Ha a lövedék egy nem-trigger (szilárd, fizikai) ütközőnek csapódik neki
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryExplode(collision.gameObject);
    }

    // --- ROBBANÁSI KÍSÉRLET ---
    // Közös ellenőrző függvény mindkét ütközési formához
    private void TryExplode(GameObject other)
    {
        if (hasExploded) return; // Ha már felrobbant ezen a képkockán, nem csinál semmit
        if (other == null) return; // Biztonsági ellenőrzés

        // Csak akkor robban fel a bájital, ha Talajhoz ("Ground"), Platformhoz ("Platform") vagy a Játékoshoz ("Player") ér.
        // Így például egy másik ellenségen vagy egy gyűjthető tárgyon simán átrepül robbanás nélkül.
        if (other.CompareTag("Ground") || other.CompareTag("Platform") || other.CompareTag("Player"))
        {
            Explode(); // Ha stimmel a címke, jöhet a tényleges robbanás
        }
    }

    // --- A TÉNYLEGES ROBBANÁS ÉS ZÓNA GENERÁLÁS ---
    private void Explode()
    {
        if (hasExploded) return; // Dupla robbanás elleni védelem
        hasExploded = true;      // Azonnal bekapcsoljuk a flag-et

        Vector3 explosionPosition = transform.position; // Elmentjük a becsapódás pontos helyét

        // Ha be van állítva a lassító zóna Prefab az Inspectorban
        if (slowZonePrefab != null)
        {
            // Létrehozzuk (legyártjuk) a lassító zónát pontosan a bájital becsapódási helyén
            GameObject zoneObject = Instantiate(slowZonePrefab, explosionPosition, Quaternion.identity);
            
            // Megkeressük a zónán lévő lassító szkriptet (SlowZone)
            SlowZone slowZone = zoneObject.GetComponent<SlowZone>();

            // Ha a zóna szkript létezik
            if (slowZone != null)
            {
                // --- ADATOK TOVÁBBADÁSA ---
                // Átadjuk az ellenségtől kapott lassítási értékeket az újonnan megszületett zónának,
                // így az most már önállóan tudja majd kezelni a játékos lassítását.
                slowZone.Initialize(slowPercent, slowDuration, slowZoneRadius);
            }
        }

        // Miután a zónát sikeresen létrehozta, a repülő bájital objektum megsemmisíti saját magát, hiszen elvégezte a dolgát
        Destroy(gameObject);
    }
}
