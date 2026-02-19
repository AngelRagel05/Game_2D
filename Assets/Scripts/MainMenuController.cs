using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to buttons in the Main Menu scene.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string levelSceneName = "Level1";

    public void PlayGame()
    {
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene(levelSceneName);
        else
            SceneManager.LoadScene(levelSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
