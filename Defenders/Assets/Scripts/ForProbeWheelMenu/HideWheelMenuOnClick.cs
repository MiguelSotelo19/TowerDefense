using UnityEngine;
using UnityEngine.EventSystems;

public class HideWheelMenuOnClick : MonoBehaviour
{
    void Update()
    {
        // Click derecho
        if (Input.GetMouseButtonDown(1))
        {
            // Si estamos sobre UI, no cerramos
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            // Cerrar menú
            WheelMenuController.Instance.HideMenu();
        }
    }
}