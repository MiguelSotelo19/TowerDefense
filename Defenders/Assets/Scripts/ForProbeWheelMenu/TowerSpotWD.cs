using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSpotWD : MonoBehaviour
{
    public static TowerSpotWD SelectedSpot;
    
    [Header("Tower References")]
    public Tower currentTower;
    
    [Header("Visual Feedback")]
    public GameObject highlightEffect;

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        SelectedSpot = this;

        Vector3 mousePos = Input.mousePosition;
        
        if (currentTower == null)
        {
            WheelMenuController.Instance.ShowBuildMenu(mousePos);
        }
        else
        {
            WheelMenuController.Instance.ShowUpgradeMenu(mousePos, currentTower);
        }
    } 
    
    public void SetTower(Tower tower)
    {
        currentTower = tower;
    }
    
    public bool IsOccupied()
    {
        return currentTower != null;
    }
    
    public void RemoveTower()
{
    if (currentTower != null)
    {
        Debug.Log($"Eliminando torre del spot");
        Destroy(currentTower.gameObject);
        currentTower = null;
    }
    else
    {
        Debug.LogWarning("No hay torre seleccionada");
    }
}
}