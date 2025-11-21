using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSpotWD : MonoBehaviour
{
    public static TowerSpotWD SelectedSpot;

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        SelectedSpot = this;

        // Usar directamente la posición del mouse en lugar de convertir world to screen
        Vector3 mousePos = Input.mousePosition;
        WheelMenuController.Instance.ShowMenu(mousePos);
    }
}