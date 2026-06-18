using UnityEngine;

// Ez a szkript egy tündért lebegtet fel-le a helyén (díszlet/hangulat céljából),
// és közben két sprite között váltogat, hogy úgy nézzen ki, mintha a szárnyaival csapkodna.
public class FairyFloat : MonoBehaviour
{
    // --- INSPECTORBAN BEÁLLÍTHATÓ TULAJDONSÁGOK ---

    [Header("Floating")]
    [SerializeField] private float floatAmplitude = 0.25f;  // Milyen magasra-mélyre lebegjen a kiindulási ponttól (kitérés mértéke)
    [SerializeField] private float floatSpeed = 2f;         // Milyen gyorsan lebegjen fel-le

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer spriteRenderer; // A tündér képmegjelenítő komponense
    [SerializeField] private Sprite tunder1;                // Az egyik szárnyállás képe (emelkedéskor)
    [SerializeField] private Sprite tunder2;                // A másik szárnyállás képe (süllyedéskor)

    // --- PRIVÁT BELSŐ VÁLTOZÓK ---

    private Vector3 startPosition;                          // A kiinduló pozíció, amelyhez képest fel-le lebeg

    // --- INITIALIZATION (Inicializálás) ---
    private void Start()
    {
        // Elmentjük az induló pozíciót, mert ehhez képest fogunk fel-le mozogni
        startPosition = transform.position;

        // Ha nem adtuk meg a SpriteRenderert, megpróbáljuk a sajátunkat használni
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // --- FŐ LOGIKA (Minden képkockán) ---
    private void Update()
    {
        // Egy folyamatosan növekvő "hullám" érték az eltelt időből és a sebességből
        float wave = Time.time * floatSpeed;

        // A szinusz hullám -1 és 1 között ingadozik, ezzel mozgatjuk a tündért fel-le
        float offsetY = Mathf.Sin(wave) * floatAmplitude;
        transform.position = startPosition + new Vector3(0f, offsetY, 0f);

        // Ha minden kép adott, a mozgás iránya szerint váltogatjuk a két szárnyállást
        if (spriteRenderer != null && tunder1 != null && tunder2 != null)
        {
            // A koszinusz a szinusz "meredeksége": pozitív, amíg emelkedik, negatív, amíg süllyed.
            // Ez alapján döntjük el, melyik szárnyállás képét mutassuk.
            bool movingUp = Mathf.Cos(wave) >= 0f;
            spriteRenderer.sprite = movingUp ? tunder1 : tunder2;
        }
    }
}
