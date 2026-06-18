using UnityEngine;

public class CameraSprintZoom : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    [SerializeField] private Camera cam;                     // A kamera komponens referenciája, amit módosítani fogunk
    [SerializeField] private PlayerMovement playerMovement; // Referencia a játékos mozgás szkriptjére a sprintelés figyeléséhez

    [SerializeField] private float normalSize = 5f;          // A kamera alapértelmezett mérete (látómezeje), amikor a játékos sétál vagy áll
    [SerializeField] private float sprintSize = 6.5f;        // A kamera megnövelt mérete, amikor a játékos sprintel (hogy távolabbról lássuk a pályát)
    [SerializeField] private float zoomSpeed = 3f;           // Az átmenet sebessége a normál és a nagyított nézet között

    // --- INITIALIZATION (Inicializálás) ---
    private void Awake()
    {
        // Ha az Inspectorban nem lett kézzel behúzva a kamera, 
        // automatikusan megpróbálja lekérni az objektumról, amin ez a szkript van
        if (cam == null)
            cam = GetComponent<Camera>();
    }

    // --- FŐ LOGIKA (Minden képkockán) ---
    private void Update()
    {
        // Biztonsági ellenőrzés (Guard Clause): Ha a kamera vagy a játékos mozgás szkriptje hiányzik,
        // azonnal megszakítjuk a futást, így elkerüljük a NullReferenceException hibákat.
        if (cam == null || playerMovement == null) return;

        // Háromtagú (ternary) operátor: Megvizsgáljuk, hogy a játékos éppen sprintel-e.
        // Ha igen (true), a célméret a sprintSize lesz, ha nem (false), akkor a normalSize.
        float targetSize = playerMovement.IsSprinting ? sprintSize : normalSize;
        
        // Mathf.Lerp (Lineáris interpoláció): Simított, fokozatos átmenetet képez a kamera jelenlegi mérete 
        // és a kiszámított célméret (targetSize) között az idő (Time.deltaTime) és a beállított sebesség függvényében.
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
    }
}
