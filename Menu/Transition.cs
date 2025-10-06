using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    public CanvasGroup fadePanel;         // Assign your dark panel's CanvasGroup here
    public float fadeDuration = 1f;       // Duration of fade effect
    public string nextSceneName;          // Name of the scene to load
    public GameObject childToDeactivate;  // Assign the child GameObject to deactivate

    private bool isFading = false;

    void OnEnable()
    {
        // Only fade in when this GameObject is activated
        fadePanel.alpha = 1;
        fadePanel.blocksRaycasts = true;
        StartCoroutine(Fade(0));
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

        // Deactivate specified child GameObject if we faded to 0 (invisible)
        if (targetAlpha == 0 && childToDeactivate != null)
        {
            childToDeactivate.SetActive(false);
        }
    }

    public void TriggerSceneChange()
    {
        if (!isFading)
        {
            gameObject.SetActive(true); // Ensure the object is active before starting fade
            StartCoroutine(FadeAndLoadScene());
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        isFading = true;
        yield return StartCoroutine(Fade(1)); // Fade to black
        SceneManager.LoadScene(nextSceneName); // Load the next scene
    }
}
