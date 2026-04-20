using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string firstLevelSceneName = "Level1";

    public void PlayGame()
    {
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
