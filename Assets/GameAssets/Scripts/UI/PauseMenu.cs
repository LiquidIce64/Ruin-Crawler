using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public bool PauseGame;
    public GameObject pauseGameMenu;
    private InputSystem_Actions input;
    private bool isLocked;
    [SerializeField] private Selectable selectFirst;

    private void Awake()
    {
        input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        input.UI.Enable();

        input.UI.Cancel.performed += OnCancelPressed;
    }

    private void OnDisable()
    {
        input.UI.Cancel.performed -= OnCancelPressed;
        input.UI.Disable();
    }

    private void OnCancelPressed(InputAction.CallbackContext context)
    {
        if (isLocked)
            return;

        Debug.Log("Escape (Cancel) нажат!");
        TogglePause();
    }

    private void TogglePause()
    {
        if (isLocked)
            return;

        if (PauseGame)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Resume()
    {
        if (isLocked)
            return;

        pauseGameMenu.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PauseGame = false;
    }

    public void Pause()
    {
        if (isLocked)
            return;

        pauseGameMenu.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PauseGame = true;
        if (selectFirst != null) selectFirst.Select();
    }

    public void LockPause()
    {
        isLocked = true;
        if (pauseGameMenu != null)
            pauseGameMenu.SetActive(false);

        PauseGame = false;
    }

    public void SelectLevel()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene("LevelSelect");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }
}
