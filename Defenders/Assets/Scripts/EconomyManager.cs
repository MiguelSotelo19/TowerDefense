using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Starting Economy")]
    [SerializeField] private int startingBytes = 500;

    [Header("Runtime")]
    public int totalBytes = 0;

    private void Awake()
    {
        Instance = this;
    }
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        InitializeEconomy();
    }

    public void InitializeEconomy()
    {
        totalBytes = startingBytes;
        Debug.Log($"Economía inicializada con {totalBytes} bytes");

        EventManager.Invoke<int>(GlobalEvents.BytesUpdated, totalBytes);
    }

    public void ResetEconomy(int newStartingBytes)
    {
        startingBytes = newStartingBytes;
        InitializeEconomy();
    }

    public int GetBytes()
    {
        return totalBytes;
    }

    public void AddBytes(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        totalBytes += amount;

        EventManager.Invoke<int>(GlobalEvents.BytesUpdated, totalBytes);
    }

    public bool SpendBytes(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (totalBytes >= amount)
        {
            totalBytes -= amount;

            EventManager.Invoke<int>(GlobalEvents.BytesUpdated, totalBytes);
            return true;
        }
        else
        {
            Debug.LogWarning($"No tienes suficientes bytes");
            return false;
        }
    }

    public bool CanAfford(int amount)
    {
        return totalBytes >= amount;
    }

    public void ShowEconomyInfo()
    {
        Debug.Log($"=== ECONOMÍA ===\nBytes actuales: {totalBytes}\nBytes iniciales: {startingBytes}");
    }
}