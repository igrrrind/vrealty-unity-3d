using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject exitConfirm;
    public GameObject optionsMenu;
    [SerializeField] private GameObject _backgroundMusic;
    private AudioLowPassFilter _lowPass;
    public bool isPaused;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    public void PauseGame()
    {

        mainMenu.SetActive(true);
        exitConfirm.SetActive(false);
        optionsMenu.SetActive(false);
        _lowPass.cutoffFrequency = 300f;
        //Time.timeScale = 0f;
        isPaused = true;

    }
    public void ResumeGame()
    {
        mainMenu.SetActive(false);
        exitConfirm.SetActive(false);
        optionsMenu.SetActive(false);
        _lowPass.cutoffFrequency = 22000f;
        //Time.timeScale = 1f;
        isPaused = false;
    }
    public void OpenOptions()
    {
        optionsMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void QuitConfirm()
    {
        exitConfirm.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void OpenQuitConfirm()
    {
        mainMenu.SetActive(false);
        exitConfirm.SetActive(true);
    }
    public void Return()
    {
        exitConfirm.SetActive(false);
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
        //_player.active = false;

    }
    public void LoadDemo()
    {
        SceneManager.UnloadSceneAsync("MainMenuScene");
        SceneManager.LoadSceneAsync("ArchViz");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
