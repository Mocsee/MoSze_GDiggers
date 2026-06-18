using UnityEngine;

// Ezek az attribútumok garantálják, hogy ha ezt a szkriptet rátesszük egy objektumra, 
// a Unity automatikusan hozzáadja a PlayerHealth és PlayerMovement komponenseket is, ha még nem lennének rajta.
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerMovement))]
public class FallDamage : MonoBehaviour
{
    [Header("Fall Damage")]
    // Hány egységnyi (méter) zuhanás után vonjon le egy teljes szívecskét a játékostól
    [SerializeField] private float distancePerHeart = 8f;

    // Referenciák a többi szükséges saját szkriptünkhöz
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    // Az ugrás/esés során elért legmagasabb Y koordináta (magassági pont)
    private float highestY;
    // Segédváltozó, ami megjegyzi, hogy az előző képkockában a földön állt-e a karakter
    private bool wasGrounded = true;

    // --- INITIALIZATION (Inicializálás) ---
    private void Awake()
    {
        // Összegyűjtjük a komponenseket ugyanarról az objektumról
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        
        // Kezdő magasság beállítása az aktuális pozícióra
        highestY = transform.position.y;
    }

    // --- LOGIKA ---
    private void Update()
    {
        // Biztonsági ellenőrzés: ha a távolság 0 vagy negatív, a zuhanási sebzés ki van kapcsolva
        if (distancePerHeart <= 0f) return;

        // Lekérjük a PlayerMovement szkripttől, hogy a földön áll-e éppen a karakter
        bool grounded = playerMovement.IsGrounded();

        // HA A LEVEGŐBEN VAN (Nem talajon áll)
        if (!grounded)
        {
            // Ha az aktuális magassága nagyobb, mint az eddig elmentett legmagasabb pont,
            // akkor frissítjük a legmagasabb pontot. (Így a csúcspontot fogja megjegyezni).
            if (transform.position.y > highestY)
                highestY = transform.position.y;
        }
        // HA A FÖLDÖN VAN (Talajon áll)
        else
        {
            // Ha az előző képkockában még a levegőben volt, de most a földön van: ez a FÖLDET ÉRÉS pillanata!
            if (!wasGrounded)
            {
                // Kiszámítjuk a tiszta zuhanási távolságot: a legmagasabb pontból kivonjuk a földet érési pontot
                float fallen = highestY - transform.position.y;
                
                // Kiszámoljuk, hány szívet kell levonni: elosztjuk a távolságot a beállított egységgel, 
                // a Mathf.FloorToInt pedig lefelé kerekíti egész számmá (pl. 1.8-ból 1 szív lesz).
                int hearts = Mathf.FloorToInt(fallen / distancePerHeart);
                
                // Ha a sebzés nagyobb, mint 0, akkor meghívjuk a PlayerHealth sebződés funkcióját
                if (hearts > 0)
                    playerHealth.TakeFallDamage(hearts);
            }
            
            // Miután földet értünk, a legmagasabb pontot folyamatosan az aktuális szinten tartjuk, amíg újra el nem ugrik
            highestY = transform.position.y;
        }

        // Elmentjük a mostani állapotot, hogy a következő képkockában (Update-ben) ez legyen a "múltbeli" (wasGrounded) állapot
        wasGrounded = grounded;
    }
}
