using UnityEngine;

public class LevelCompletePickup : MonoBehaviour
{
    [SerializeField] private FinishScreenManager finishScreenManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (finishScreenManager != null)
        {
            finishScreenManager.CompleteLevel();
        }

        gameObject.SetActive(false);
    }
}
