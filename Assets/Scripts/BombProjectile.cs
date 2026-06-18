using UnityEngine;

public class BombProjectile : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    // A bomba maximális élettartama másodpercben. Ha nem ütközik semmivel, ennyi idő után automatikusan megsemmisül.
    [SerializeField] private float lifetime = 5f;

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private Vector2 moveDirection; // A repülés iránya (normalizált vektor)
    private float moveSpeed;       // A repülés sebessége
    private bool initialized;      // Logikai változó, ami jelzi, hogy a bomba megkapta-e már a kezdősebességét

    // --- MEGHÍVHATÓ INICIALIZÁLÓ FÜGGVÉNY ---
    // Ezt a függvényt a BombEnemy szkript hívja meg közvetlenül a bomba létrehozása (Instantiate) után.
    public void Initialize(Vector2 direction, float speed)
    {
        moveDirection = direction.normalized; // Biztosítja, hogy az irányvektor hossza pontosan 1 legyen
        moveSpeed = speed;                   // Beállítja a repülési sebességet
        initialized = true;                  // Engedélyezi a mozgást az Update-ben
    }

    // --- INDULÁSI LOGIKA ---
    private void Start()
    {
        // Unity beépített funkció: A megadott idő (lifetime) letelte után automatikusan 
        // törli ezt az objektumot a játékból, megakadályozva, hogy a végtelenbe repülő bombák lelassítsák a számítógépet.
        Destroy(gameObject, lifetime);
    }

    // --- FŐ MOZGÁSI KÖR (Minden képkockán) ---
    private void Update()
    {
        // Ha a bomba még nincs inicializálva (nem kapott irányt), nem mozdul el
        if (!initialized) return;

        // Egyenes vonalban mozgatja a bombát a kiszámított irányba és sebességgel, az időtől függően (Time.deltaTime)
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    // --- TALÁLAT ÉS ROBBANÁS KEZELÉSE ---
    // Akkor fut le, ha a bomba Trigger típusú ütközője (Collider2D) hozzáér egy másik objektum ütközőjéhez
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ha a bomba a játékoshoz ("Player") ér, nem történik semmi, a bomba átrepül rajta.
        // (A játékos védelmét vagy magát a sebzést egy másik rendszer kezeli, vagy a bomba kifejezetten csak környezetrombolásra van).
        if (other.CompareTag("Player"))
            return;

        // --- PLATFORM ROMBOLÁS ---
        // Ha a bomba egy rombolható platformba ("Platform") csapódik
        if (other.CompareTag("Platform"))
        {
            Destroy(other.gameObject); // Megsemmisíti a platformot (eltünteti a pályáról a blokkot)
            Destroy(gameObject);       // Megsemmisíti saját magát is (a bomba elhasználódik/felrobban)
            return;                    // Megszakítja a függvény további futását
        }

        // --- TALAJBA CSAPÓDÁS ---
        // If a bomba a sima talajba ("Ground") csapódik, ami a játékban valószínűleg elpusztíthatatlan falnak számít
        if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);       // Csak a bombát semmisíti meg, a talajblokk épségben marad
        }
    }
}
