using UnityEngine;

public class TowerSpot : MonoBehaviour
{
    [Header("Spot Status")]
    public bool isAvailable = true;
    
    [Header("Tower Configuration")]
    public GameObject placedTower;
    
    [Header("Visual Settings")]
    public Vector3 placementOffset = new Vector3(0, 0.5f, 0);
    
    [Tooltip("Color cuando está disponible")]
    public Color availableColor = new Color(0, 1, 0, 0.5f);
    
    [Tooltip("Color cuando está ocupado")]
    public Color occupiedColor = new Color(1, 0, 0, 0.5f);
    
    [Tooltip("Radio de visualización del spot")]
    public float spotRadius = 0.8f;
    
    public bool PlaceTower(GameObject towerPrefab)
    {
        if (!isAvailable || placedTower != null)
            return false;
        
        placedTower = Instantiate(towerPrefab, transform.position + placementOffset, Quaternion.identity);
        placedTower.transform.SetParent(transform); 
        
        isAvailable = false;
        return true;
    }
    
    public void RemoveTower()
    {
        if (placedTower != null)
        {
            Destroy(placedTower);
            placedTower = null;
            isAvailable = true;
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = isAvailable ? availableColor : occupiedColor;
        
        Vector3 position = transform.position;
        
        DrawCircle(position, spotRadius, 20);
        
        Gizmos.DrawLine(position, position + Vector3.up * 1f);
        
        DrawCircle(position + Vector3.up * 1f, spotRadius, 20);
    }
    
    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + placementOffset, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + placementOffset);
    }

    private void OnMouseEnter()
    {
        EventManager.Invoke(GlobalEvents.MouseEnterTower, this);
    }

    private void OnMouseExit()
    {
        EventManager.Invoke(GlobalEvents.MouseExitTower, this);
    }
}
