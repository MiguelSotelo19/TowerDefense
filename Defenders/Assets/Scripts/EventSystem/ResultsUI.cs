using UnityEngine;
using TMPro;

public class ResultsUI : MonoBehaviour
{
    [Header("Stats Text")]
    public TextMeshProUGUI wavesText;
    public TextMeshProUGUI enemiesText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI bytesText;

    private void OnEnable()
    {
        UpdateStats();
    }

    private void UpdateStats()
    {
        if (GameManager.Instance == null) return;

        // Oleadas completadas
        if (wavesText != null)
        {
            int waves = GameManager.Instance.GetWavesCompleted();
            int totalWaves = WaveManager.Instance != null ? WaveManager.Instance.TotalWaves : 0;
            wavesText.text = $"Oleadas: {waves}/{totalWaves}";
        }

        // Enemigos eliminados
        if (enemiesText != null)
        {
            enemiesText.text = $"Enemigos eliminados: {GameManager.Instance.GetEnemiesKilled()}";
        }

        // Tiempo de juego
        if (timeText != null)
        {
            float playTime = GameManager.Instance.GetPlayTime();
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);
            timeText.text = $"Tiempo: {minutes:00}:{seconds:00}";
        }

        // Bytes restantes
        if (bytesText != null)
        {
            bytesText.text = $"Bytes restantes: {GameManager.Instance.GetBytesRemaining()}";
        }
    }
}