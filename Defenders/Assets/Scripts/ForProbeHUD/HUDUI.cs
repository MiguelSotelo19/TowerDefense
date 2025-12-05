using System;
using TMPro;
using UnityEngine;

public class HUDUI : MonoBehaviour 
{
    [SerializeField] private TextMeshProUGUI nucleo, oleada, bytes;

    private void OnEnable()
    {
        EventManager.Subscribe<int>(GlobalEvents.CoreHealthUpdated, OnCoreHealthUpdated);
        EventManager.Subscribe<string>(GlobalEvents.OleadasUpdated, OnOleadasUpdated);
        EventManager.Subscribe<int>(GlobalEvents.BytesUpdated, OnBytesUpdated); // AGREGAR
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<int>(GlobalEvents.CoreHealthUpdated, OnCoreHealthUpdated);
        EventManager.Unsubscribe<string>(GlobalEvents.OleadasUpdated, OnOleadasUpdated);
        EventManager.Unsubscribe<int>(GlobalEvents.BytesUpdated, OnBytesUpdated); // AGREGAR
    }

    private void OnCoreHealthUpdated(int health)
    {
        nucleo.text = health.ToString();
    }

    private void OnOleadasUpdated(string waveInfo)
    {
        oleada.text = waveInfo;
    }

    // AGREGAR ESTE MÉTODO
    private void OnBytesUpdated(int totalBytes)
    {
        bytes.text = totalBytes.ToString();
    }
}