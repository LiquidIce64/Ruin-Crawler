using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private GameObject deathScreenMenu;

    private bool isShown;

    private void Awake()
    {
        EnsureMenu();
        if (!isShown)
            deathScreenMenu.SetActive(false);
    }

    public void Show()
    {
        if (isShown)
            return;

        isShown = true;
        EnsureMenu();
        deathScreenMenu.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }

    private void EnsureMenu()
    {
        if (deathScreenMenu == null)
            deathScreenMenu = gameObject;
    }
}
