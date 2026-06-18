using NUnit.Framework;
using UnityEngine;

// 1. TESZT (Edit Mode): a pálya-feloldás / progress-reset logika (PlayerPrefs).
public class LevelSelectManagerTests
{
    // Minden teszt előtt és után töröljük a mentett haladást, hogy a tesztek
    // ne befolyásolják egymást, és a valódi mentésedet se írják felül.
    [SetUp]    public void SetUp()    => PlayerPrefs.DeleteAll();
    [TearDown] public void TearDown() => PlayerPrefs.DeleteAll();

    [Test]
    public void UnlockLevel2_beallitja_a_feloldas_flaget()
    {
        var go  = new GameObject();
        var mgr = go.AddComponent<LevelSelectManager>();

        mgr.UnlockLevel2();

        Assert.AreEqual(1, PlayerPrefs.GetInt("Level2_Unlocked", 0));

        Object.DestroyImmediate(go);
    }

    [Test]
    public void ResetProgress_ujra_lezarja_a_kesobbi_palyakat_de_a_Level1_nyitva_marad()
    {
        var go  = new GameObject();
        var mgr = go.AddComponent<LevelSelectManager>();
        mgr.UnlockLevel2();
        mgr.UnlockLevel3();

        mgr.ResetProgress();

        Assert.AreEqual(1, PlayerPrefs.GetInt("Level1_Unlocked", 0), "Level 1 mindig nyitva van");
        Assert.AreEqual(0, PlayerPrefs.GetInt("Level2_Unlocked", 0), "Level 2 ujra lezarva");
        Assert.AreEqual(0, PlayerPrefs.GetInt("Level3_Unlocked", 0), "Level 3 ujra lezarva");

        Object.DestroyImmediate(go);
    }
}
