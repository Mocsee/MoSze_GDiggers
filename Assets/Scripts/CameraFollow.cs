using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    [SerializeField] private Transform target;             // A célpont (általában a játékos), akit a kamerának követnie kell
    [SerializeField] private PlayerMovement playerMovement; // Referencia a játékos mozgás szkriptjére a sprintelés figyeléséhez

    [Header("Follow")]
    [SerializeField] private float smoothSpeed = 3f;        // A kamera mozgásának simítottsága (Lerp sebessége), minél kisebb, annál "lágyszabban" követ
    [SerializeField] private float yOffset = 2f;            // Függőleges eltolás, hogy a karakter ne pont a képernyő közepén, hanem kicsit lejjebb legyen

    [Header("Zoom")]
    [SerializeField] private float normalSize = 50f;        // A kamera alapértelmezett mérete (látómezeje) séta/állás közben
    [SerializeField] private float sprintSize = 60f;        // A kamera megnövelt mérete sprintelés közben (távolabbi nézet)
    [SerializeField] private float zoomSpeed = 5f;          // A zoom átmenet sebessége (milyen gyorsan váltson a két nézet között)

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private Camera cam;                                     // A kamerakomponens saját referenciája
    private float minY;                                     // A kamera által elérhető legalsó Y koordináta (a kiindulási magasság)

    // --- INITIALIZATION (Inicializálás) ---
    private void Awake()
    {
        // Lekéri a kamerakomponenst erről az objektumról
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        // Elmenti az induló Y pozíciót, mint abszolút minimum magasságot
        minY = transform.position.y;

        // Ha a kamera komponens létezik, beállítja az alapméretet
        if (cam != null)
            cam.orthographicSize = normalSize;
    }

    // A LateUpdate a fizikai és sima mozgások után fut le, így elkerülhető a kamera és a játékos rángatózása
    private void LateUpdate()
    {
        // Biztonsági ellenőrzés: ha nincs kit követni, vagy nincs kamera, nem csinál semmit
        if (target == null || cam == null) return;

        FollowOnlyUp(); // Meghívja a csak felfelé követő logikát
        HandleZoom();   // Meghívja a dinamikus zoom logikát
    }

    // --- KAMERA MOZGÁS LOGIKA ---
    private void FollowOnlyUp()
    {
        // Kiszámítja a kívánt Y pozíciót: a célpont (játékos) aktuális magassága + az eltolás
        float targetY = target.position.y + yOffset;

        // Mathf.Lerp (Lineáris interpoláció): Simított átmenetet képez a jelenlegi kamera Y pozíció és a cél Y pozíció között.
        // Ez biztosítja, hogy a kamera ne mereven, hanem lágyan, késleltetve kövesse a karaktert.
        float newY = Mathf.Lerp(transform.position.y, targetY, smoothSpeed * Time.deltaTime);
        
        // Módosítja a kamera pozícióját az új Y értékkel, miközben az X és Z értékeket változatlanul hagyja
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // "Only Up" (Csak felfelé) korlátozás: Ha a kiszámított új pozíció kisebb lenne, mint a pálya elején elmentett minimum magasság,
        // akkor a kamera pozícióját kényszerítve visszaállítja a minY értékre. Így a kamera sosem süllyedhet a kezdőpont alá.
        if (transform.position.y < minY)
        {
            transform.position = new Vector3(transform.position.x, minY, transform.position.z);
        }
    }

    // --- KAMERA ZOOM LOGIKA ---
    private void HandleZoom()
    {
        if (playerMovement == null) return; // Biztonsági ellenőrzés

        // Ternary (háromtagú) operátor: Ha a játékos éppen sprintel (IsSprinting = true), akkor a sprintSize-t választja célméretnek, 
        // különben a normalSize-t.
        float targetSize = playerMovement.IsSprinting ? sprintSize : normalSize;
        
        // Simított átmenettel (Mathf.Lerp) közelíti a kamera ortografikus méretét a kívánt célmérethez
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
    }
}
