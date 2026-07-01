using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool PauseGame;
    public GameObject pauseGameMenu;
    private InputSystem_Actions input;

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
        Debug.Log("Escape (Cancel) нажат!");
        TogglePause();
    }

    private void TogglePause()
    {
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
        pauseGameMenu.SetActive(false);
        Time.timeScale = 1f;
        PauseGame = false;
    }

    public void Pause()
    {
        pauseGameMenu.SetActive(true);
        Time.timeScale = 0f;
        PauseGame = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}