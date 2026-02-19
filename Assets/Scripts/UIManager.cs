using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the HUD. Assign the Text/TextMeshPro elements in the Inspector.
/// Call UpdateUI() from GameManager whenever values change.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI timerText;

    private float elapsedTime = 0f;
    private bool timerRunning = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(elapsedTime / 60f);
                int seconds = Mathf.FloorToInt(elapsedTime % 60f);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = "Lives: " + lives;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }
}
