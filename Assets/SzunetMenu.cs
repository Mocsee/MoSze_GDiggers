using UnityEngine; // Beemeli a Unity alapvető parancskészletét.
using UnityEngine.SceneManagement; // Lehetővé teszi a pályák közötti váltást és újratöltést.

public class SzünetMenü : MonoBehaviour // Létrehozza az osztályt (a fájlnévnek SzünetMenü.cs-nek kell lennie).
{
    [SerializeField] GameObject szunetPanel; // Létrehoz egy helyet az Inspectorban, ahová be tudod húzni a szünet menü grafikáját.

    void Update() // Ez a funkció minden egyes képkockánál (másodpercenként kb. 60-szor) lefut.
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Megnézi, hogy ebben a pillanatban lenyomtad-e az ESC billentyűt.
        {
            if (szunetPanel.activeSelf) // Megvizsgálja, hogy a szünet panel éppen látható-e a képernyőn.
            {
                Folytat(); // Ha látható, akkor meghívja a Folytat funkciót (bezárja a menüt).
            }
            else // Ha a panel éppen nem látható...
            {
                Szünet(); // ...akkor meghívja a Szünet funkciót (megnyitja a menüt).
            }
        }
    }

    public void Szünet() // Ez a funkció állítja meg a játékot.
    {
        szunetPanel.SetActive(true); // Bekapcsolja a szünet panel objektumot, hogy látszódjon.
        Time.timeScale = 0f; // Teljesen megállítja az időt a játékban (minden mozgás és fizika megfagy).
    }

    public void Folytat() // Ez a funkció visz vissza a játékba.
    {
        szunetPanel.SetActive(false); // Kikapcsolja a szünet panelt, így az eltűnik.
        Time.timeScale = 1f; // Visszaállítja az időt normál sebességre.
    }

    public void Otthon() // Ez a funkció tölti be a főmenüt.
    {
        Time.timeScale = 1f; // Kilépés előtt fontos visszaállítani az időt, különben a menü is "fagyott" maradna.
        SceneManager.LoadScene("Main Menu"); // Betölti a "Main Menu" nevű jelenetet.
    }

    public void Ujrakezd() // Ez a funkció indítja újra az aktuális szintet.
    {
        Time.timeScale = 1f; // Újratöltés előtt is visszaállítjuk az idő sebességét.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Lekéri a mostani pálya sorszámát és újra betölti azt.
    }
}