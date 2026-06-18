using NUnit.Framework;
using UnityEngine;

// 2. TESZT (Edit Mode): a pálya teljesítése feloldja a következő pályát ÉS lefagyasztja a játékot.
public class FinishScreenManagerTests
{
    [SetUp]    public void SetUp()    { PlayerPrefs.DeleteAll(); Time.timeScale = 1f; }
    [TearDown] public void TearDown() { PlayerPrefs.DeleteAll(); Time.timeScale = 1f; }

    [Test]
    public void CompleteLevel_feloldja_a_kovetkezo_palyat_es_lefagyaszt()
    {
        var go  = new GameObject();
        var fsm = go.AddComponent<FinishScreenManager>();
        // A nextLevelName alapertelmezesben "Level2" (igy van a scriptben inicializalva),
        // ezert nem kell semmit kezzel beallitani.

        fsm.CompleteLevel();

        Assert.AreEqual(1, PlayerPrefs.GetInt("Level2_Unlocked", 0), "feloldja a kovetkezo palyat");
        Assert.AreEqual(0f, Time.timeScale, "a jatek lefagy (timeScale = 0)");

        Object.DestroyImmediate(go);
    }
}
