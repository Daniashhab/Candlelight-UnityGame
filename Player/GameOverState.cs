using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverState : MonoBehaviour
{
    // Called by the "Restart" button
    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // Called by the "Main Menu" button
    public void MainMenu()
    {
        SceneManager.LoadScene("Mainmenu"); // Make sure the MainMenu scene is added in Build Settings
    }

    // Optional: Called by the "Quit" button
    public void QuitGame()
    {
        Application.Quit();

        // Optional for debugging in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
