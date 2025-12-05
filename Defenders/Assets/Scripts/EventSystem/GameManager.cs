using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    private bool gameEnded = false;
    private float gameStartTime;
    private bool isPaused = false;


    [Header("Statistics")]
    public int enemiesKilled = 0;
    public int wavesCompleted = 0;
    
    [Header("UI References")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public GameObject pausePanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        gameStartTime = Time.time;
        
        // Ocultar paneles al inicio
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

    }

    private void OnEnable()
    {
        EventManager.Subscribe(GlobalEvents.AllWavesCompleted, OnVictory);
        EventManager.Subscribe<Enemy>(GlobalEvents.EnemyDied, OnEnemyKilled);
        EventManager.Subscribe<int>(GlobalEvents.WaveCompleted, OnWaveCompleted);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe(GlobalEvents.AllWavesCompleted, OnVictory);
        EventManager.Unsubscribe<Enemy>(GlobalEvents.EnemyDied, OnEnemyKilled);
        EventManager.Unsubscribe<int>(GlobalEvents.WaveCompleted, OnWaveCompleted);
    }

    private void OnEnemyKilled(Enemy enemy)
    {
        enemiesKilled++;
    }

    private void OnWaveCompleted(int waveNumber)
    {
        wavesCompleted = waveNumber;
    }

    public void OnVictory()
    {
        if (gameEnded) return;
        gameEnded = true;
        isPaused = false;

        Debug.Log("¡VICTORIA!");
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0f; // Pausar juego
        }
    }

    public void OnDefeat()
    {
        if (gameEnded) return;
        gameEnded = true;
        isPaused = false;

        Debug.Log("¡DERROTA!");
        
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
            Time.timeScale = 0f; // Pausar juego
        }
    }

    // Métodos para botones de UI
    public void RestartGame()
    {
        Time.timeScale = 1f; // Reanudar tiempo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f; // Reanudar tiempo
        // Cambiar "MainMenu" por el nombre de tu escena de menú
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void TogglePause()
    {
        // Evitar pausar después de victoria/derrota
        if (gameEnded) return;

        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;

            if (pausePanel != null)
                pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;

            if (pausePanel != null)
                pausePanel.SetActive(false);
        }
    }


    // Getters para las estadísticas
    public int GetEnemiesKilled() => enemiesKilled;
    public int GetWavesCompleted() => wavesCompleted;
    public float GetPlayTime() => Time.time - gameStartTime;
    public int GetBytesRemaining() => EconomyManager.Instance != null ? EconomyManager.Instance.GetBytes() : 0;
}