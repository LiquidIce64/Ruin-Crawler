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
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void EnsureMenu()
    {
        if (deathScreenMenu == null)
            deathScreenMenu = gameObject;
    }
}
