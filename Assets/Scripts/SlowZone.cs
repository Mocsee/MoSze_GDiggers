using UnityEngine;

public class SlowZone : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    [SerializeField] private float lifeTime = 2f;                   // Mennyi ideig létezzen maga a lassító zóna a pályán, mielőtt eltűnik
    [SerializeField] private CircleCollider2D circleCollider;       // A kör alakú ütköző (Trigger), ami a zóna hatósugarát reprezentálja
    [SerializeField] private ParticleSystem blueExplosionEffect;    // Kék robbanási/köd részecske effekt, ami vizuálisan jelzi a zónát

    // --- PRIVÁT BELSŐ VÁLTOZÓK (Alapértelmezett értékekkel, amiket az inicializálás felülír) ---

    private float slowPercent = 0.5f;   // A lassítás mértéke (0.5 = az eredeti sebesség 50%-ára lassít)
    private float slowDuration = 2f;    // Hány másodpercig tartson a lassító hatás a játékoson
    private float radius = 1.5f;        // A lassító kör sugara
    private bool initialized;           // Logikai változó, ami jelzi, hogy a zóna megkapta-e már a paramétereit

    // --- MEGHÍVHATÓ INICIALIZÁLÓ FÜGGVÉNY ---
    // Ezt a PotionProjectile hívja meg, miután sikeresen legyártotta ezt a zónát a becsapódás helyén
    public void Initialize(float newSlowPercent, float newSlowDuration, float newRadius)
    {
        slowPercent = newSlowPercent;   // Átveszi a lassítás mértékét
        slowDuration = newSlowDuration; // Átveszi a lassítás időtartamát
        radius = newRadius;             // Átveszi a zóna méretét
        initialized = true;             // Flag bekapcsolása
        
        SetupZone();                    // Elindítja a zóna fizikai és vizuális beállításait
    }

    private void Start()
    {
        // Üresen hagyva, mivel az inicializálást és az indulást a SetupZone() intézi manuálisan
    }

    // --- A ZÓNA AKTIVÁLÁSA ÉS BEÁLLÍTÁSA ---
    private void SetupZone()
    {
        // Beállítja a kör alakú ütköző méretét a bájitaltól kapott pontos sugárra
        if (circleCollider != null)
            circleCollider.radius = radius;

        // Vizuális effekt biztonságos elindítása
        if (blueExplosionEffect != null)
        {
            blueExplosionEffect.transform.localPosition = Vector3.zero; // Középre igazítás
            // Ha már futott valamiért, leállítja és kitörli a meglévő részecskéket
            blueExplosionEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            blueExplosionEffect.Clear();
            blueExplosionEffect.Play(); // Elindítja a kék köd/robbanás effektet
        }

        // --- AZONNALI ELLENŐRZÉS ---
        // Megvizsgálja, hogy a létrehozás pillanatában már benne áll-e a játékos a zónában
        ApplySlowToPlayersInside();
        
        // Beidőzíti a lassító zóna teljes megsemmisítését a megadott élettartam (lifeTime) után
        Destroy(gameObject, lifeTime);
    }

    // --- RÁGORDULÓ JÁTÉKOSOK LASSÍTÁSA ---
    // Akkor fut le, ha a játékos a zóna létezése alatt KÉSŐBB sétál bele a kör területébe
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return; // Ha még nincs inicializálva, biztonsági okokból nem csinál semmit
        if (!other.CompareTag("Player")) return; // Csak a játékosra reagál

        // Lekéri a PlayerMovement szkriptet a belépő játékosról
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            // Meghívja a játékos saját lassító függvényét, átadva a százalékot és az időtartamot
            playerMovement.ApplySlow(slowPercent, slowDuration);
        }
    }

    // --- BELÜL ÁLLÓ JÁTÉKOSOK LASSÍTÁSA (Spawn pillanatában) ---
    // Ez a függvény egy láthatatlan fizikai kört rajzol a zóna köré a spawn másodpercében, 
    // és mindenkit elkap, aki már eleve ott tartózkodott.
    private void ApplySlowToPlayersInside()
    {
        // Physics2D.OverlapCircleAll: Visszaad egy tömböt az összes olyan Collider2D-ről, 
        // ami a megadott pozícióban és sugárban (radius) található a fizikai térben.
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        // Végigmegy az összes talált ütközőn
        for (int i = 0; i < hits.Length; i++)
        {
            // Ha a talált objektum nem a játékos, ugorja át és nézze a következőt
            if (!hits[i].CompareTag("Player")) continue;

            // Ha megtalálta a játékost a körön belül, lekéri a mozgás szkriptjét
            PlayerMovement playerMovement = hits[i].GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                // Azonnal alkalmazza rá a lassítást
                playerMovement.ApplySlow(slowPercent, slowDuration);
            }
        }
    }
}
