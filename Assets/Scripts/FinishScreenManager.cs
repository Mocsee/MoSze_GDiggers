using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishScreenManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject finishScreenUI;
    [SerializeField] private GameObject winScreenUI;
    [SerializeField] private bool isLastLevel = false;

    [Header("Scenes")]
    [SerializeField] private string currentLevelName = "Level1";
    [SerializeField] private string nextLevelName = "Level2";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    private bool levelCompleted = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (finishScreenUI != null)
            finishScreenUI.SetActive(false);

        if (winScreenUI != null)
            winScreenUI.SetActive(false);
    }

    public void CompleteLevel()
    {
        if (levelCompleted) return;

        levelCompleted = true;

        if (!string.IsNullOrEmpty(nextLevelName))
        {
            PlayerPrefs.SetInt(nextLevelName + "_Unlocked", 1);
            PlayerPrefs.Save();
        }

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (isLastLevel)
        {
            if (winScreenUI != null)
                winScreenUI.SetActive(true);
        }
        else
        {
            if (finishScreenUI != null)
                finishScreenUI.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelName);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
