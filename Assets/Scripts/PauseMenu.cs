using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenu;
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;

    private bool isPaused = false;
    private CursorLockMode previousLockMode;
    private bool previousVisible;

    void Start()
    {
        if(resumeButton) resumeButton.onClick.AddListener(Resume);
        if(restartButton) restartButton.onClick.AddListener(Restart);
        if(quitButton) quitButton.onClick.AddListener(Quit);

        if(level1Button) level1Button.onClick.AddListener(() => SetLevel(1));
        if(level2Button) level2Button.onClick.AddListener(() => SetLevel(2));
        if(level3Button) level3Button.onClick.AddListener(() => SetLevel(3));

        if(pauseMenu != null) pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        previousLockMode = Cursor.lockState;
        previousVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
        if(pauseMenu != null) pauseMenu.SetActive(true);
        isPaused = true;
    }

    public void Resume()
    {
        Cursor.lockState = previousLockMode;
        Cursor.visible = previousVisible;

        Time.timeScale = 1f;
        if(pauseMenu != null) pauseMenu.SetActive(false);
        isPaused = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void SetLevel(int level)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(level);
    }
}