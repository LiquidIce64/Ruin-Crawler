using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const string tutorialScene = "MGameScene";

    public void PlayGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (SaveManager.saveData.completedLevels.Contains(tutorialScene))
            SceneManager.LoadScene("LevelSelect");
        else
            SceneManager.LoadScene(tutorialScene);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game");
        Application.Quit();
    }
}
