using UnityEngine;

public class TowerSpot : MonoBehaviour
{
    [Header("Spot Status")]
    [Tooltip("¿Está disponible para colocar torre?")]
    public bool isAvailable = true;
    
    [Header("Tower Configuration")]
    [Tooltip("Torre actualmente colocada (null si vacío)")]
    public GameObject placedTower;
    
    [Header("Visual Settings")]
    [Tooltip("Offset vertical para colocar la torre")]
    public Vector3 placementOffset = new Vector3(0, 0.5f, 0);
    
    [Tooltip("Color cuando está disponible")]
    public Color availableColor = new Color(0, 1, 0, 0.5f); // Verde semi-transparente
    
    [Tooltip("Color cuando está ocupado")]
    public Color occupiedColor = new Color(1, 0, 0, 0.5f); // Rojo semi-transparente
    
    [Tooltip("Radio de visualización del spot")]
    public float spotRadius = 0.8f;
    
    // Método para colocar una torre
    public bool PlaceTower(GameObject towerPrefab)
    {
        if (!isAvailable || placedTower != null)
            return false;
        
        // Instanciar torre en la posición del spot + offset
        placedTower = Instantiate(towerPrefab, transform.position + placementOffset, Quaternion.identity);
        placedTower.transform.SetParent(transform); // Hacer hijo del spot
        
        isAvailable = false;
        return true;
    }
    
    // Método para vender/quitar torre
    public void RemoveTower()
    {
        if (placedTower != null)
        {
            Destroy(placedTower);
            placedTower = null;
            isAvailable = true;
        }
    }
    
    // Visualización en el editor
    void OnDrawGizmos()
    {
        Gizmos.color = isAvailable ? availableColor : occupiedColor;
        
        // Dibujar cilindro (aproximado con círculos)
        Vector3 position = transform.position;
        
        // Base
        DrawCircle(position, spotRadius, 20);
        
        // Altura
        Gizmos.DrawLine(position, position + Vector3.up * 1f);
        
        // Top
        DrawCircle(position + Vector3.up * 1f, spotRadius, 20);
    }
    
    // Método auxiliar para dibujar círculos
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
    
    // Visualizar en Scene View cuando está seleccionado
    void OnDrawGizmosSelected()
    {
        // Dibujar línea hacia arriba mostrando el offset de colocación
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + placementOffset, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + placementOffset);
    }
}
