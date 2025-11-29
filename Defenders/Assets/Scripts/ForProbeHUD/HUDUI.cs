using System;
using TMPro;
using UnityEngine;

public class HUDUI : MonoBehaviour 
{
    [SerializeField] private TextMeshProUGUI nucleo, oleada, bytes;

    private void OnEnable()
    {
        EventManager.Subscribe<int>(GlobalEvents.CoreHealthUpdated, OnCoreHealthUpdated);
        EventManager.Subscribe<string>(GlobalEvents.OleadasUpdated, OnOleadasUpdated); // COMPLETAR ESTA LÍNEA
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<int>(GlobalEvents.CoreHealthUpdated, OnCoreHealthUpdated);
        EventManager.Unsubscribe<string>(GlobalEvents.OleadasUpdated, OnOleadasUpdated);
    }

    private void OnCoreHealthUpdated(int health)
    {
        nucleo.text = health.ToString();
    }

    private void OnOleadasUpdated(string waveInfo)
    {
        oleada.text = waveInfo; // Mostrará "1/10", "2/10", etc.
    }
}