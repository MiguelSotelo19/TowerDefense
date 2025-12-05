using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Starting Economy")]
    [SerializeField] private int startingBytes = 500; // Bytes iniciales del nivel

    [Header("Runtime")]
    public int totalBytes = 0;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Inicializar con los bytes de inicio
        InitializeEconomy();
    }

    /// <summary>
    /// Inicializa la economía del nivel con los bytes iniciales
    /// </summary>
    public void InitializeEconomy()
    {
        totalBytes = startingBytes;
        Debug.Log($"💰 Economía inicializada con {totalBytes} bytes");

        // Notificar a la UI
        EventManager.Invoke<int>(GlobalEvents.BytesUpdated, totalBytes);
    }

    /// <summary>
    /// Reinicia la economía (útil al cambiar de nivel)
    /// </summary>
    public void ResetEconomy(int newStartingBytes)
    {
        startingBytes = newStartingBytes;
        InitializeEconomy();
    }

    /// <summary>
    /// Obtiene la cantidad actual de bytes
    /// </summary>
    public int GetBytes()
    {
        return totalBytes;
    }

    /// <summary>
    /// Agrega bytes (por matar enemigos, vender torres, etc.)
    /// </summary>
    public void AddBytes(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("⚠️ Intentando agregar cantidad negativa o cero de bytes");
            return;
        }

        totalBytes += amount;
        Debug.Log($"💵 +{amount} bytes. Total: {totalBytes}");

        // Notificar a la UI
        EventManager.Invoke<int>(GlobalEvents.BytesUpdated, totalBytes);
    }

    /// <summary>
    /// Gasta bytes (construir torres, upgrades, etc.)
    /// Retorna true si se pudo gastar, false si no hay suficientes
    /// </summary>
    public bool SpendBytes(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("⚠️ Intentando gastar cantidad negativa o cero de bytes");
            return false;
        }

        if (totalBytes >= amount)
        {
            totalBytes -= amount;
            Debug.Log($"💸 -{amount} bytes. Restante: {totalBytes}");

            // Notificar a la UI
            EventManager.Invoke<int>(GlobalEvents.BytesUpdated, totalBytes);
            return true;
        }
        else
        {
            Debug.LogWarning($"❌ No tienes suficientes bytes. Necesitas {amount}, tienes {totalBytes}");
            return false;
        }
    }

    /// <summary>
    /// Verifica si se pueden gastar cierta cantidad de bytes sin gastarlos
    /// </summary>
    public bool CanAfford(int amount)
    {
        return totalBytes >= amount;
    }

    /// <summary>
    /// Muestra información de economía en consola (debug)
    /// </summary>
    public void ShowEconomyInfo()
    {
        Debug.Log($"=== ECONOMÍA ===\nBytes actuales: {totalBytes}\nBytes iniciales: {startingBytes}");
    }
}