using UnityEngine;
// Ez a sor behúzza a Unity alapvető eszközkészletét.

using UnityEngine.SceneManagement;
// Ez a sor teszi lehetővé, hogy válthassunk a pályák (Scene-ek) között.

public class MainMenu : MonoBehaviour
// Itt adjuk meg a script nevét, ami mindenképp egyezzen meg a fájlneveddel.
{
    public void Inditas()
    // Ez a nyilvános funkció fogja elindítani a játékot, ha megnyomod a gombot.
    {
        SceneManager.LoadSceneAsync(1);
        // Ez a parancs betölti a Build Settings-ben 1-es számmal jelölt pályát.
    }

    public void Kilepes()
    // Ezt a funkciót tudod hozzárendelni a piros X gombhoz a kilépéshez.
    {
        Application.Quit();
        // Ez a parancs bezárja a játékot (de csak a kiexportált, kész verzióban).
    }
}