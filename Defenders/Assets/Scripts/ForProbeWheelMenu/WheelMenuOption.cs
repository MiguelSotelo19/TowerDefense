using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WheelMenuOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tower Settings")]
    public GameObject towerPrefab;
    public int towerCost = 100;
    
    [Header("Visual Feedback")]
    public float hoverScale = 1.2f;
    public float scaleSpeed = 10f;
    public Color hoverColor = new Color(1f, 1f, 1f, 1f);
    
    private Vector3 originalScale;
    private Color originalColor;
    private Image image;
    private Vector3 targetScale;
    private Color targetColor;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        
        image = GetComponent<Image>();
        if (image != null)
        {
            originalColor = image.color;
            targetColor = originalColor;
        }
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        
        if (image != null)
        {
            image.color = Color.Lerp(image.color, targetColor, Time.deltaTime * scaleSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
        targetColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        targetColor = originalColor;
    }

    public void BuildTower()
    {
        if (TowerSpotWD.SelectedSpot == null)
            return;
        
        // Verificar si ya hay una torre
        if (TowerSpotWD.SelectedSpot.IsOccupied())
        {
            Debug.LogWarning("Ya hay una torre en este spot!");
            return;
        }
        
        // Verificar dinero (opcional, si tienes EconomyManager)
        if (EconomyManager.Instance != null && EconomyManager.Instance.GetBytes() < towerCost)
        {
            Debug.LogWarning("No hay suficientes bytes!");
            return;
        }
        
        // Pagar torre
        if (EconomyManager.Instance != null)
        {
            if (!EconomyManager.Instance.SpendBytes(towerCost))
            {
                Debug.LogWarning("No tienes suficientes bytes!");
                return; // Salir sin construir
            }
        }

        // Instanciar torre en el spot
        GameObject towerGO = Instantiate(towerPrefab, 
                                        TowerSpotWD.SelectedSpot.transform.position, 
                                        Quaternion.identity);
        
        // ← ESTO ES LO QUE FALTABA ←
        // Registrar torre en el spot
        Tower tower = towerGO.GetComponent<Tower>();
        if (tower != null)
        {
            TowerSpotWD.SelectedSpot.SetTower(tower);
            Debug.Log($"Torre '{tower.towerName}' registrada en el spot");
        }
        else
        {
            Debug.LogError("El prefab no tiene el componente Tower!");
        }

        // Ocultar el menú
        WheelMenuController.Instance.HideMenu();
    }
}