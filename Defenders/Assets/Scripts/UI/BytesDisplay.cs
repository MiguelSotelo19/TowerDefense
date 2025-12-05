using UnityEngine;
using TMPro;

public class BytesDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bytesText;

    private void OnEnable()
    {
        EventManager.Subscribe<int>(GlobalEvents.BytesUpdated, UpdateDisplay);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<int>(GlobalEvents.BytesUpdated, UpdateDisplay);
    }

    private void UpdateDisplay(int bytes)
    {
        if (bytesText != null)
        {
            bytesText.text = $"Bytes: {bytes}";
        }
    }
}