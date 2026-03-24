using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// Ezek a sorok teszik lehetővé a Unity alapfunkcióinak, a pályakezelésnek és a UI elemeknek a használatát.

public class SzintMenü : MonoBehaviour
{
    public Button[] gombok;
    // Ez a lista (tömb) fogja tárolni az összes szintválasztó gombot.

    public GameObject szintGombokSzuloje;
    // Ide kell behúznod a Unity-ben azt a panelt vagy objektumot, ami alatt a gombok vannak.

    private void Awake()
    // Ez a funkció rögtön a menü betöltésekor lefut.
    {
        GombokatListaba();
        // Először meghívja az automatikus gombkereső funkciót.

        int kijatszottszint = PlayerPrefs.GetInt("UnlockedLevel", 1);
        // Lekéri a mentett haladást a memóriából (ha nincs, az 1. szint lesz az alapértelmezett).

        for (int i = 0; i < gombok.Length; i++)
        // Végigmegy az összes megtalált gombon.
        {
            gombok[i].interactable = false;
            // Alaphelyzetben minden gombot lelakatol.
        }

        for (int i = 0; i < kijatszottszint; i++)
        // Újra végigmegy a gombokon, de csak addig, ameddig a játékos eljutott.
        {
            gombok[i].interactable = true;
            // Ezeket a szinteket elérhetővé teszi.
        }
    }

    void GombokatListaba()
    // Ez a funkció végzi el a gombok automatikus összegyűjtését.
    {
        int gyerekekSzama = szintGombokSzuloje.transform.childCount;
        // Megszámolja, hány darab objektum (gomb) van a szülő objektum alatt.

        gombok = new Button[gyerekekSzama];
        // Létrehoz egy akkora listát (tömböt), amiben pontosan elfér az összes gomb.

        for (int i = 0; i < gyerekekSzama; i++)
        // Sorban végigmegy az összes gyereke elemen.
        {
            gombok[i] = szintGombokSzuloje.transform.GetChild(i).gameObject.GetComponent<Button>();
            // Kikeresi a soron következő gombot és beteszi a listánkba.
        }
    }

    public void OpenLevel(int szintId)
    // Ez a funkció tölti be a kiválasztott szintet.
    {
        string szintNeve = "Szint" + szintId;
        // Összeállítja a pálya nevét (pl. "Szint1").

        SceneManager.LoadScene(szintNeve);
        // Megparancsolja a Unity-nek a pálya betöltését.
    }
}