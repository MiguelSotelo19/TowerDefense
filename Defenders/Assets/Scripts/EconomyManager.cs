using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public int totalBytes = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public int GetBytes()
    {
        return totalBytes;
    }

    public void AddBytes(int amount)
    {
        totalBytes += amount;
        Debug.Log($"Ganaste {amount} bytes. Total: {totalBytes}");
    }

    public bool SpendBytes(int amount)
    {
        if (totalBytes >= amount)
        {
            totalBytes -= amount;
            Debug.Log($"Gastaste {amount} bytes. Restante: {totalBytes}");
            return true;
        }
        else
        {
            Debug.Log("No tienes suficientes bytes.");
            return false;
        }
    }
}
