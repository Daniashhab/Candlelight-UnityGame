using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Assign VideoPlayer in Inspector
    public string videoFileName;    // Example: "first_cutscene_ver1.mp4"
    public string nextSceneName;    // Set next scene name in Inspector
    public Canvas cutsceneCanvas;   // Assign Canvas in Inspector

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // Get the correct path to StreamingAssets
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);

        // Convert Windows backslashes to forward slashes for compatibility
        videoPath = videoPath.Replace("\\", "/");

        // Fix for Android (needs "file://")
        if (Application.platform == RuntimePlatform.Android)
        {
            videoPath = "file://" + videoPath;
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            videoPath = "file:///" + videoPath; // Ensure triple forward slashes for Windows
        }

        // Assign the corrected URL
        videoPlayer.url = videoPath;
        videoPlayer.Play();

        // Detect when video ends
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            LoadNextScene();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("No next scene specified. Deactivating canvas.");
            if (cutsceneCanvas != null)
            {
                cutsceneCanvas.gameObject.SetActive(false);
            }
        }
    }
}
