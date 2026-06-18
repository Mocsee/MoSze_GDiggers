using System.Collections;
using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    [Header("Chase")]
    [SerializeField] private float moveSpeed = 3f;         // Az ellenség mozgási sebessége üldözés közben
    [SerializeField] private float detectionRange = 8f;     // Észlelési hatósugár (milyen közel kell mennie a játékosnak, hogy üldözni kezdje)

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 2.5f;   // A hátralökés ereje, amikor az ellenség nekitámad a játékosnak
    [SerializeField] private float knockbackDuration = 0.2f; // A hátralökési effekt időtartama másodpercben

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private Transform player;          // Referencia a játékos pozíciójához (Transform komponenséhez)
    private bool isDead = false;       // Halott-e már az ellenség (true/false)
    private bool isKnockedBack = false; // Éppen hátralökődik-e az ellenség a játékossal való ütközés miatt

    // --- INITIALIZATION (Inicializálás) ---
    private void Start()
    {
        // Megkeresi a játékban az "Player" címkével (Tag) ellátott objektumot
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        
        // Ha megtalálta a játékost, elmenti a transform (pozíció) referenciáját
        if (playerObject != null)
            player = playerObject.transform;
    }

    // --- FŐ AI LOGIKA (Minden képkockán) ---
    private void Update()
    {
        if (isDead) return;        // Ha az ellenség halott, nem csinál semmit
        if (isKnockedBack) return; // Ha éppen hátralökődés fázisban van, a fizika irányítja, nem az AI mozgás
        if (player == null) return; // Ha nincs meg a játékos, nem tud mit üldözni

        // Kiszámítja a pontos távolságot az ellenség és a játékos aktuális pozíciója között a 2D térben
        float distance = Vector2.Distance(transform.position, player.position);

        // HA A JÁTÉKOS AZ ÉSZLELÉSI HATÓSUGÁRON BELÜL VAN
        if (distance <= detectionRange)
        {
            // Elkészít egy célpozíciót a játékos X és Y koordinátái alapján, de megtartja az ellenség saját Z mélységét
            Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
            
            // Vector3.MoveTowards: Egyenes vonalban mozgatja az ellenséget az aktuális helyéről a célpozíció felé,
            // a megadott sebességgel, az idő függvényében (Time.deltaTime)
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    // --- ÜTKÖZÉS KEZELÉSE ---
    // Akkor fut le, ha egy másik objektum belép az ellenség Trigger típusú ütközőjébe (Collider2D)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return; // Ha halott, nem sebez
        
        // Ellenőrzi, hogy a belépő objektum tag-je (címkéje) "Player"-e. Ha nem az, figyelmen kívül hagyja
        if (!other.CompareTag("Player")) return;

        // Megpróbálja lekérni a PlayerHealth szkriptet a játékosról
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Megsebzi a játékost, átadva neki az ellenség pozícióját (hogy a játékos tudja, merre kell hátralökődnie)
            playerHealth.TakeDamage(transform.position);
            
            // Az ellenséget is meglöki a játékossal ellentétes irányba, hogy ne ragadjanak egymásba
            KnockbackAwayFromPlayer(other.transform.position);
        }
    }

    // --- ELLENSÉG HÁTRALÖKÉSI EFFEKTUSA ---
    private void KnockbackAwayFromPlayer(Vector3 playerPosition)
    {
        // Elindítja a hátralökés időzített folyamatát (Coroutine)
        StartCoroutine(KnockbackCoroutine(playerPosition));
    }

    // Időzített hátralökési folyamat
    private IEnumerator KnockbackCoroutine(Vector3 playerPosition)
    {
        isKnockedBack = true; // Bekapcsolja a hátralökési állapotot, így az Update-ben az üldözés átmenetileg leáll

        // Kiszámítja a hátralökés irányát: az ellenség pozíciójából kivonja a játékos pozícióját,
        // így a kapott vektor pontosan a játékostól elfelé fog mutatni
        Vector3 direction = (transform.position - playerPosition).normalized;
        float timer = 0f; // Időzítő inicializálása

        // Amíg a hátralökési idő le nem telik
        while (timer < knockbackDuration)
        {
            // Folyamatosan tolja az ellenséget a kiszámított irányba az erőnek megfelelően
            transform.position += direction * knockbackForce * Time.deltaTime;
            
            timer += Time.deltaTime; // Növeli az időzítőt
            yield return null;       // Vár a következő képkockáig (így a mozgás sima lesz, nem akad meg a játék)
        }

        isKnockedBack = false; // A folyamat végén kikapcsolja a hátralökési állapotot, az AI újra üldözhet
    }

    // --- HALÁL LOGIKA ---
    // Külső szkriptek (pl. a fejre ugrást kezelő szkript) meghívhatják ezt, ha megölik az ellenséget
    public void Die()
    {
        if (isDead) return; // Ha már halott, nem csinál semmit (dupla halál elkerülése)
        
        isDead = true;
        Destroy(gameObject); // Véglegesen törli ezt az ellenség objektumot a játékból
    }
}
