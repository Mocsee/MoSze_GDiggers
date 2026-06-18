using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// 3. TESZT (Play Mode): sebzeskor eggyel csokken az elet.
// [UnityTest] + IEnumerator = a teszt a tenyleges play-loopban fut (lefut az Awake, lehet "varni" egy frame-et).
public class PlayerHealthTests
{
    [UnityTest]
    public IEnumerator TakeFallDamage_eggyel_csokkenti_az_eletet()
    {
        // Inaktivan hozzuk letre, hogy az Awake LEFUTASA ELOTT beallithassuk a privat mezot.
        var go = new GameObject("Player");
        go.SetActive(false);
        var health = go.AddComponent<PlayerHealth>();

        // Ures tomb, hogy az UpdateLivesUI ne dobjon NullReference-t (nincs UI a teszt-jelenetben).
        SetPrivateField(health, "lifeImages", new Image[0]);

        go.SetActive(true);   // Most fut le az Awake -> currentLives = maxLives (3).
        yield return null;    // Varunk egy frame-et, hogy az Awake biztosan lefusson.

        health.TakeFallDamage(1);

        int lives = (int)GetPrivateField(health, "currentLives");
        Assert.AreEqual(2, lives);

        Object.Destroy(go);
    }

    // Reflection, mert a currentLives privat mezo.
    // Tisztabb alternativa: tegyel a PlayerHealth-be egy "public int CurrentLives => currentLives;" sort,
    // es akkor ez a ket segedfuggveny elhagyhato.
    static void SetPrivateField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);

    static object GetPrivateField(object target, string name) =>
        target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
}
