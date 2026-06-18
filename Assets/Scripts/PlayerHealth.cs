using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // A pálya újratöltéséhez szükséges névtér
using UnityEngine.UI;             // Az UI (felhasználói felület) elemek, mint pl. az Image kezeléséhez kell

public class PlayerHealth : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    [Header("Lives")]
    [SerializeField] private int maxLives = 3;         // Maximális életek (szívecskék) száma az induláskor
    [SerializeField] private Image[] lifeImages;       // UI képek tömbje, amik a szívecskéket jelenítik meg a képernyőn

    [Header("Damage")]
    [SerializeField] private float invincibilityTime = 1f; // Sérthetetlenségi idő (másodpercben) sérülés után (i-frames)
    [SerializeField] private float bounceForce = 12f;      // A hátralökés (knockback) ereje, amikor a karakter megsebződik
    [SerializeField] private float movementLockTime = 0.25f; // Mennyi időre veszítse el a játékos az irányítást sebződéskor (bénulás)

    [Header("Effects")]
    [SerializeField] private ParticleSystem damageEffect; // Sebződéskor elinduló részecske-effekt (pl. vér, szikrák vagy por)

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private int currentLives;          // Az aktuális életek száma a játék során
    private bool isInvincible;         // Éppen sérthetetlen-e a játékos (true/false)
    private Rigidbody2D body;          // Referencia a fizikai testhez a hátralökés kiszámításához
    private PlayerMovement playerMovement; // Referencia a mozgás szkriptre, hogy sebződéskor le tudjuk tiltani az irányítást

    // --- INITIALIZATION (Inicializálás) ---

    private void Awake()
    {
        // Komponensek begyűjtése a karakterről
        body = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        
        // Aktuális életek beállítása a maximumra a játék kezdetén
        currentLives = maxLives;
        
        // A képernyőn lévő szívecskék kijelzésének frissítése
        UpdateLivesUI();
    }

    // --- SEBZŐDÉSI LOGIKÁK ---

    // Alapvető sebződés, amikor egy ellenség eltalálja a játékost
    public void TakeDamage(Vector2 enemyPosition)
    {
        // Ha a játékos éppen sérthetetlen (pl. nemrég sebződött), nem történik semmi
        if (isInvincible) return;

        currentLives--;    // Élet csökkentése eggyel
        UpdateLivesUI();   // UI frissítése (eltűnik egy szívecske)
        PlayDamageEffect(); // Részecske effekt lejátszása

        // --- HÁTRALÖKÉS (Knockback) KISZÁMÍTÁSA ---
        // Kiszámítja a löökés irányát: a játékos pozíciójából kivonja az ellenség pozícióját, így pontosan ellentétes irányba fog repülni
        Vector2 bounceDirection = ((Vector2)transform.position - enemyPosition).normalized;
        
        // Biztosítja, hogy a hátralökés mindig feldobja egy kicsit a karaktert az Y tengelyen, ne csak oldalra tolja
        bounceDirection.y = Mathf.Abs(bounceDirection.y) + 0.5f;

        body.linearVelocity = Vector2.zero; // Nullázza az aktuális sebességet, hogy a hátralökés tiszta erőből érvényesüljön
        // Impulzus szerű erőt fejt ki a karakterre a kiszámított irányba
        body.AddForce(bounceDirection.normalized * bounceForce, ForceMode2D.Impulse);

        // Átmenetileg letiltja a játékos mozgásirányítását, hogy ne tudjon azonnal "ellenkormányozni" a hátralökésnek
        if (playerMovement != null)
            playerMovement.DisableMovementTemporarily(movementLockTime);

        // Ha elfogytak az életek, meghal a karakter, és megszakítjuk a függvény futását
        if (currentLives <= 0)
        {
            Die();
            return;
        }

        // Elindítja a sérthetetlenségi időszakot mérő folyamatot
        StartCoroutine(InvincibilityCoroutine());
    }

    // Zuhanási sebzés kezelése (külön függvény, mert itt nincs ellenség pozíció, ami hátralökne)
    public void TakeFallDamage(int hearts)
    {
        if (hearts <= 0) return; // Biztonsági ellenőrzés

        currentLives -= hearts; // Levonja a megadott mennyiségű életet
        UpdateLivesUI();        // UI frissítése
        PlayDamageEffect();     // Effekt lejátszása

        // Halál ellenőrzése
        if (currentLives <= 0)
            Die();
    }

    // Külsőleg meghívható függvény (pl. az EnemyStompHitbox-ból), ami feldobja a karaktert, ha sikeresen rálépett egy ellenség fejére
    public void BounceUpAfterStomp(float stompBounceForce)
    {
        // Megtartja az X irányú sebességet, de az Y (függőleges) sebességet a megadott ugrási erőre kényszeríti
        body.linearVelocity = new Vector2(body.linearVelocity.x, stompBounceForce);
    }

    // --- VIZUÁLIS EFFEKTEK ÉS IDŐZÍTŐK ---

    // A sebződési részecskék biztonságos elindítása
    private void PlayDamageEffect()
    {
        if (damageEffect == null) return;

        // Ha már futott az effekt, leállítja, kitörli a meglévő részecskéket, majd újraindítja az elejéről
        damageEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        damageEffect.Clear();
        damageEffect.Play();
    }

    // Sérthetetlenségi időzítő (Coroutine)
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true; // Bekapcsolja a sérthetetlenséget
        yield return new WaitForSeconds(invincibilityTime); // Várakozás a megadott ideig (pl. 1 másodperc)
        isInvincible = false; // Kikapcsolja a sérthetetlenséget
    }

    // --- UI ÉS JELENET KEZELÉS ---

    // Frissíti a képernyőn látható szívecskéket
    private void UpdateLivesUI()
    {
        // Végigmegy az összes beállított szívecske képen
        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] != null)
            {
                // Ha az index kisebb, mint az aktuális életek száma, a kép látható marad (enabled = true), különben eltűnik (false)
                // Pl. ha 2 életünk van: a 0. és 1. indexű kép be lesz kapcsolva, a 2. indexű ki lesz kapcsolva.
                lifeImages[i].enabled = i < currentLives;
            }
        }
    }

    // Halál logika: Ha a karakter meghal, újraindítja az aktuális pályát az elejéről
    private void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
