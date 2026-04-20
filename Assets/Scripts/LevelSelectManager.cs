using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Level Buttons")]
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;

    [Header("Debug")]
    [SerializeField] private bool resetProgressOnStart = false;

    private void Start()
    {
        if (resetProgressOnStart)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        if (!PlayerPrefs.HasKey("Level1_Unlocked"))
        {
            PlayerPrefs.SetInt("Level1_Unlocked", 1);
            PlayerPrefs.Save();
        }

        UpdateLevelButtons();
    }

    private void UpdateLevelButtons()
    {
        if (level1Button != null)
            level1Button.interactable = PlayerPrefs.GetInt("Level1_Unlocked", 0) == 1;

        if (level2Button != null)
            level2Button.interactable = PlayerPrefs.GetInt("Level2_Unlocked", 0) == 1;

        if (level3Button != null)
            level3Button.interactable = PlayerPrefs.GetInt("Level3_Unlocked", 0) == 1;
    }

    public void PlayHighestUnlockedLevel()
    {
        if (PlayerPrefs.GetInt("Level3_Unlocked", 0) == 1)
        {
            SceneManager.LoadScene("Level3");
        }
        else if (PlayerPrefs.GetInt("Level2_Unlocked", 0) == 1)
        {
            SceneManager.LoadScene("Level2");
        }
        else
        {
            SceneManager.LoadScene("Level1");
        }
    }

    public void LoadLevel1()
    {
        if (PlayerPrefs.GetInt("Level1_Unlocked", 0) == 1)
            SceneManager.LoadScene("Level1");
    }

    public void LoadLevel2()
    {
        if (PlayerPrefs.GetInt("Level2_Unlocked", 0) == 1)
            SceneManager.LoadScene("Level2");
    }

    public void LoadLevel3()
    {
        if (PlayerPrefs.GetInt("Level3_Unlocked", 0) == 1)
            SceneManager.LoadScene("Level3");
    }

    public void UnlockLevel2()
    {
        PlayerPrefs.SetInt("Level2_Unlocked", 1);
        PlayerPrefs.Save();
        UpdateLevelButtons();
    }

    public void UnlockLevel3()
    {
        PlayerPrefs.SetInt("Level3_Unlocked", 1);
        PlayerPrefs.Save();
        UpdateLevelButtons();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("Level1_Unlocked", 1);
        PlayerPrefs.Save();
        UpdateLevelButtons();
    }
}
