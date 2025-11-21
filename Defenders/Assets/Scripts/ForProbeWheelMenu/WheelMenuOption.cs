using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WheelMenuOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tower Settings")]
    public GameObject towerPrefab;
    
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
        // Animar escala suavemente
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        
        // Animar color suavemente
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

        // Instanciar torre en el spot
        Instantiate(towerPrefab, TowerSpotWD.SelectedSpot.transform.position, Quaternion.identity);

        // Ocultar el menú
        WheelMenuController.Instance.HideMenu();
    }
}