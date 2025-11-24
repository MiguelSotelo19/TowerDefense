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

            // ← CAMBIO AQUÍ ←
            // Verificar que el menú esté activo antes de ocultarlo
            if (WheelMenuController.Instance != null && 
                WheelMenuController.Instance.gameObject.activeInHierarchy)
            {
                WheelMenuController.Instance.HideMenu();
            }
        }
    }
}