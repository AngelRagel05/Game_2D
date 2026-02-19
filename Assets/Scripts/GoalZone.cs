using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Place this trigger zone at the end of the level (e.g., the Golden Feather / Exit door).
/// Set 'nextSceneName' to the name of the Win Screen or next level scene.
/// </summary>
public class GoalZone : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "WinScreen";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            // Stop the UI timer
            if (UIManager.Instance != null)
                UIManager.Instance.StopTimer();

            // Transition to win screen
            if (SceneTransition.Instance != null)
                SceneTransition.Instance.LoadScene(nextSceneName);
            else
                SceneManager.LoadScene(nextSceneName);
        }
    }
}
