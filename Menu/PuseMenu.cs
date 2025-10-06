using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PuseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private InputActionAsset candlelightAsset;
    [SerializeField] private GameObject optionsPanel;

    public AudioSource backgroundmusic;
    public Slider backgroundVolumeSlider;
    public GameObject musicSlider;
    public GameObject musicEffectSlider;

    public GameObject resolutionChanger;
    public TMP_Dropdown resDropDown;
    public Toggle fullScreenToggle;

    Resolution[] allResolutions;
    bool isFullScreen;
    int selectedResolution;
    List<Resolution> selectedResolutionList = new List<Resolution>();

    private bool isPaused;
    private InputAction pauseAction;
    public void Start()
    {
        pauseMenu.SetActive(false);

        isFullScreen = true;
        allResolutions = Screen.resolutions;

        List<string> resolutionStringList = new List<string>();
        string newRes;
        foreach (Resolution res in allResolutions)
        {
            newRes = res.width.ToString() + " x " + res.height.ToString();
            if (!resolutionStringList.Contains(newRes))
            {
                resolutionStringList.Add(newRes);
                selectedResolutionList.Add(res);
            }
        }

        resDropDown.AddOptions(resolutionStringList);

        backgroundmusic.Play();
        backgroundVolumeSlider.value = backgroundmusic.volume;
        backgroundVolumeSlider.onValueChanged.AddListener(HandelVolumeChange);
    }

    private void OnEnable()
    {
        // 1) Find the correct Action Map 
        var actionMap = candlelightAsset.FindActionMap("UI", throwIfNotFound: false);
        

        // 2) Find the action named "Pause" 
        pauseAction = actionMap?.FindAction("Pause", throwIfNotFound: false);

        // 3) Subscribe to the action 
        if (pauseAction != null)
        {
            pauseAction.performed += OnPausePerformed;
            pauseAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePerformed;
            pauseAction.Disable();
        }
    }

    // This method is called whenever the "Pause" action is performed (e.g., Esc pressed)
    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }


    public void PauseGame() 
    {
        pauseMenu.SetActive(true); 
        Time.timeScale = 0f;        
        isPaused = true;
    }
    
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
    public void ExitToMainMenu() 
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);

    }
    public void OpenOptions() 
    {
        pauseMenu.SetActive(false );
        optionsPanel.SetActive(true);
    }
    public void CloseOptions() 
    {
        pauseMenu?.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void RestartTheScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ChangeResolution()
    {
        selectedResolution = resDropDown.value;
        Screen.SetResolution(selectedResolutionList[selectedResolution].width, selectedResolutionList[selectedResolution].height, isFullScreen);
    }

    public void ChangeFullScreen()
    {
        isFullScreen = fullScreenToggle.isOn;
        Screen.SetResolution(selectedResolutionList[selectedResolution].width, selectedResolutionList[selectedResolution].height, isFullScreen);

    }
    private void HandelVolumeChange(float newVolume)
    {
        backgroundmusic.volume = newVolume;
    }
}
