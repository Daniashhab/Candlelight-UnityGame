using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public AudioSource backgroundmusic;
    public Slider backgroundVolumeSlider;
    public ParticleSystem fallingCandles;

    public GameObject musicSlider;
    public GameObject musicEffectSlider;
    public GameObject animatedPanel;
    public CanvasGroup fadePanel; // Assign this in the Inspector
    public float fadeDuration = 1f; // Duration of fade effect

    public GameObject resolutionChanger;
    public TMP_Dropdown resDropDown;
    public Toggle fullScreenToggle;

    Resolution[] allResolutions;
    bool isFullScreen;
    int selectedResolution;
    List<Resolution> selectedResolutionList = new List<Resolution>();

    private void Start()
    {
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

        if (fallingCandles != null)
        {
            fallingCandles.Play();
        }

        // Ensure the screen starts black
        fadePanel.alpha = 1;
        fadePanel.blocksRaycasts = true;

        // Start fading in
        StartCoroutine(Fade(0));
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

    public void PlayGame()
    {
        StartCoroutine(FadeToBlackAndLoadScene(1)); // Scene 3
    }

    IEnumerator FadeToBlackAndLoadScene(int sceneIndex)
    {
        yield return StartCoroutine(Fade(1)); // Fade to black
        SceneManager.LoadScene(sceneIndex);
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadePanel.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            fadePanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        fadePanel.alpha = targetAlpha;

        if (targetAlpha == 0)
        {
            fadePanel.blocksRaycasts = false; // Allow button interactions after fade-in
        }
        else
        {
            fadePanel.blocksRaycasts = true; // Block interactions during fade-out
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void HandelVolumeChange(float newVolume)
    {
        backgroundmusic.volume = newVolume;
    }

    public void OpenSettings()
    {
        animatedPanel.SetActive(true);
        Invoke("OnAnimationEnd", 2f);
    }

    public void CloseSettings()
    {
        animatedPanel.SetActive(false);
        musicSlider.SetActive(false);
        musicEffectSlider.SetActive(false);
        resolutionChanger.SetActive(false);
        fullScreenToggle.gameObject.SetActive(false);
    }



    public void OnAnimationEnd()
    {
        musicSlider.SetActive(true);
        musicEffectSlider.SetActive(true);
        resolutionChanger.SetActive(true);
        fullScreenToggle.gameObject.SetActive(true);
    }
}