using UnityEngine;

public class DamageProjectile : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    // A lövedék maximális élettartama másodpercben. Ha nem talál el semmit, ennyi idő után magától megsemmisül.
    [SerializeField] private float lifetime = 5f;

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private Vector2 moveDirection; // A lövedék repülési iránya
    private float moveSpeed;       // A lövedék repülési sebessége
    private bool initialized;      // Logikai változó, ami jelzi, hogy a lövedék megkapta-e a kezdősebességét az ellenségtől

    // --- MEGHÍVHATÓ INICIALIZÁLÓ FÜGGVÉNY ---
    // Ezt a függvényt a DamageEnemy szkript hívja meg közvetlenül a lövedék létrehozásakor (Instantiate).
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
        // törli ezt a lövedéket a játékból, hogy ne terhelje feleslegesen a memóriát.
        Destroy(gameObject, lifetime);
    }

    // --- FŐ MOZGÁSI KÖR (Minden képkockán) ---
    private void Update()
    {
        // Ha a lövedék még nincs inicializálva (nem kapott irányt és sebességet), nem mozdul el
        if (!initialized) return;

        // Egyenes vonalban, állandó sebességgel mozgatja a lövedéket a kiszámított irányba az idő függvényében (Time.deltaTime)
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    // --- TALÁLAT ÉS SEBZÉS KEZELÉSE ---
    // Akkor fut le, ha a lövedék Trigger típusú ütközője (Collider2D) hozzáér egy másik objektum ütközőjéhez
    private void OnTriggerEnter2D(Collider2D other)
    {
        // --- JÁTÉKOS ELTALÁLÁSA ---
        // Ha a lövedék a játékoshoz ("Player") ér
        if (other.CompareTag("Player"))
        {
            // Megpróbálja lekérni a PlayerHealth szkriptet a játékosról
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Megsebzi a játékost, átadva neki a lövedék aktuális pozícióját (ebből tudja a játékos, merre kell hátralökődnie)
                playerHealth.TakeDamage(transform.position);
            }

            // Megsemmisíti saját magát (a lövedék elnyelődik/eltűnik a találat után)
            Destroy(gameObject);
            return; // Megszakítja a függvény további futását
        }

        // --- KÖRNYEZETBE CSAPÓDÁS ---
        // Ha a lövedék egy platformba ("Platform") vagy a sima talajba ("Ground") csapódik
        if (other.CompareTag("Platform") || other.CompareTag("Ground"))
        {
            // Egyszerűen megsemmisíti saját magát, a környezetben nem tesz kárt
            Destroy(gameObject);
        }
    }
}
