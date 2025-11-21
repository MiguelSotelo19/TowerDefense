using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private GameObject victoryPanel;
    
    private void OnEnable()
    {
        EventManager.Subscribe<int>(GlobalEvents.WaveStarted, OnWaveStarted);
        EventManager.Subscribe<int>(GlobalEvents.WaveCompleted, OnWaveCompleted);
        EventManager.Subscribe(GlobalEvents.AllWavesCompleted, OnAllWavesCompleted);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<int>(GlobalEvents.WaveStarted, OnWaveStarted);
        EventManager.Unsubscribe<int>(GlobalEvents.WaveCompleted, OnWaveCompleted);
        EventManager.Unsubscribe(GlobalEvents.AllWavesCompleted, OnAllWavesCompleted);
    }

    private void Start()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
        UpdateWaveText();
    }

    private void OnWaveStarted(int waveNumber)
    {
        UpdateWaveText();
    }

    private void OnWaveCompleted(int waveNumber)
    {
        // Podrías mostrar un mensaje temporal aquí
        Debug.Log($"UI: Oleada {waveNumber} completada");
    }

    private void OnAllWavesCompleted()
    {
        if (waveText != null)
        {
            waveText.text = "¡VICTORIA!";
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    private void UpdateWaveText()
    {
        if (WaveManager.Instance != null && waveText != null)
        {
            waveText.text = $"Oleada {WaveManager.Instance.CurrentWave}/{WaveManager.Instance.TotalWaves}";
        }
    }
}