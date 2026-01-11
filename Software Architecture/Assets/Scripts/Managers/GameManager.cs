using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Over")]
    public bool pauseOnGameOver = true;
    public GameOverUI gameOverUI;

    bool gameOver;

    void Awake()
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

    public void HandlePlayerDied()
    {
        if (gameOver) return;
        gameOver = true;

        if (pauseOnGameOver)
            Time.timeScale = 0f;

        // Let the player use the UI / see cursor on game over.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameOverUI != null)
            gameOverUI.Show();
        else
            Debug.LogWarning("GameOverUI reference missing on GameManager.");
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        gameOver = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

