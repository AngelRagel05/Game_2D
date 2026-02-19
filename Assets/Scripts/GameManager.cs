using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject playerPrefab;

    [Header("Lives")]
    [SerializeField] private int maxLives = 3;

    private GameObject currentPlayer;
    private int score = 0;
    private int lives;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        lives = maxLives;
        UpdateUI();
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (spawnPoint != null && playerPrefab != null)
        {
            currentPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        }
    }

    public void RespawnPlayer()
    {
        lives--;
        UpdateUI();

        if (lives <= 0)
        {
            GameOver();
            return;
        }

        if (currentPlayer != null)
            Destroy(currentPlayer);

        SpawnPlayer();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.UpdateLives(lives);
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over!");
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene("GameOver");
        else
            SceneManager.LoadScene("GameOver");
    }
}
