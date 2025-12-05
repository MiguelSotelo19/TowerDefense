using UnityEngine;
using UnityEngine.EventSystems;

public class HoverHUD : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Feedback")]
    public float hoverScale = 1.2f;
    public float scaleSpeed = 10f;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    
    void Start()
    {
        // Guardar la escala original
        originalScale = transform.localScale;
        targetScale = originalScale;
    }
    
    void Update()
    {
        // Interpolar suavemente hacia la escala objetivo
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Al pasar el mouse encima, aumentar escala
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Al salir el mouse, volver a escala original
        targetScale = originalScale;
    }
}