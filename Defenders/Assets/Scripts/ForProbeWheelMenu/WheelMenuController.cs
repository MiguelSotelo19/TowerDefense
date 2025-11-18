using UnityEngine;
using System.Collections;

public class WheelMenuController : MonoBehaviour
{
    public static WheelMenuController Instance;

    [Header("Animation Settings")]
    public float animationDuration = 0.2f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine currentAnimation;

    private void Awake()
    {
        Instance = this;
        
        // Obtener componentes necesarios
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        rectTransform = GetComponent<RectTransform>();
        
        gameObject.SetActive(false);
    }

    public void ShowMenu(Vector3 screenPos)
    {
        // Posicionar el menú
        rectTransform.position = screenPos;
        
        // Activar y animar
        gameObject.SetActive(true);
        
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
            
        currentAnimation = StartCoroutine(AnimateMenu(true));
    }

    public void HideMenu()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
            
        currentAnimation = StartCoroutine(AnimateMenu(false));
    }

    private IEnumerator AnimateMenu(bool show)
    {
        float elapsed = 0f;
        float startScale = show ? 0f : 1f;
        float endScale = show ? 1f : 0f;
        float startAlpha = show ? 0f : 1f;
        float endAlpha = show ? 1f : 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveValue = scaleCurve.Evaluate(t);

            // Animar escala
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, curveValue);
            
            // Animar transparencia
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);

            yield return null;
        }

        // Asegurar valores finales
        transform.localScale = Vector3.one * endScale;
        canvasGroup.alpha = endAlpha;

        if (!show)
            gameObject.SetActive(false);
    }
}