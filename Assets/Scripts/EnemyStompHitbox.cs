using UnityEngine;

public class EnemyStompHitbox : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    [SerializeField] private EnemyChase enemy;             // Referencia a fő ellenség szkriptre, amit meg kell semmisíteni taposáskor
    [SerializeField] private float stompBounceForce = 14f; // Az erő, amivel a játékos feldobódik/visszapattan a sikeres taposás után

    // --- ÜTKÖZÉS KEZELÉSE ---
    // Akkor fut le, ha egy másik objektum belép a fejre helyezett Trigger típusú ütközőbe
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ellenőrzi, hogy a belépő objektum tag-je (címkéje) "Player"-e. Ha nem, azonnal megszakítja a futást.
        if (!other.CompareTag("Player")) return;

        // Megpróbálja lekérni a játékos fizikai testét (Rigidbody2D) és az életerő szkriptjét (PlayerHealth)
        Rigidbody2D playerBody = other.GetComponent<Rigidbody2D>();
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        // Biztonsági ellenőrzés: Ha a játékos komponensei vagy a célpont ellenség hivatkozása hiányzik, nem csinál semmit
        if (playerBody == null || playerHealth == null || enemy == null) return;

        // --- A TAPOSÁS KULCSFELTÉTELE ---
        // Megvizsgálja a játékos függőleges (Y) sebességét. 
        // Ha a sebesség kisebb mint 0, az azt jelenti, hogy a játékos éppen ZUHAN (lefelé mozog a levegőben).
        // Ez akadályozza meg, hogy az ellenség akkor is meghaljon, ha a játékos alulról ugrik neki a lábának.
        if (playerBody.linearVelocity.y < 0f)
        {
            // Meghívja a játékos életerő szkriptjének a bounce funkcióját, ami feldobja őt a magasba
            playerHealth.BounceUpAfterStomp(stompBounceForce);
            
            // Meghívja az ellenség Die() függvényét, ami megsemmisíti az ellenséget a pályáról
            enemy.Die();
        }
    }
}
