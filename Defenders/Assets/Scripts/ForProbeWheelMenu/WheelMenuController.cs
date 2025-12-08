using UnityEngine;
using System.Collections;

public class WheelMenuController : MonoBehaviour
{
    public static WheelMenuController Instance;

    [Header("Animation Settings")]
    public float animationDuration = 0.2f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Menu References")]
    public GameObject buildOptionsContainer;
    public GameObject upgradeOptionsContainer;
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine currentAnimation;
    private Tower selectedTower;

    private void Awake()
    {
        Instance = this;
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        rectTransform = GetComponent<RectTransform>();
        
        gameObject.SetActive(false);
    }

    public void ShowBuildMenu(Vector3 screenPos)
    {
        selectedTower = null;
        
        gameObject.SetActive(true);
        
        if (buildOptionsContainer != null)
            buildOptionsContainer.SetActive(true);
        if (upgradeOptionsContainer != null)
            upgradeOptionsContainer.SetActive(false);
        
        ShowMenu(screenPos);
    }
    
    public void ShowUpgradeMenu(Vector3 screenPos, Tower tower)
    {
        selectedTower = tower;
        
        gameObject.SetActive(true);
        
        if (buildOptionsContainer != null)
            buildOptionsContainer.SetActive(false);
        if (upgradeOptionsContainer != null)
            upgradeOptionsContainer.SetActive(true);
        
        UpdateUpgradeUI(tower);
        ShowMenu(screenPos);
    }

    private void ShowMenu(Vector3 screenPos)
    {
        rectTransform.position = screenPos;
        
        // gameObject.SetActive(true);
        
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
            
        currentAnimation = StartCoroutine(AnimateMenu(true));
    }

    public void HideMenu()
    {
        selectedTower = null;
        
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

            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, curveValue);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);

            yield return null;
        }

        transform.localScale = Vector3.one * endScale;
        canvasGroup.alpha = endAlpha;

        if (!show)
            gameObject.SetActive(false);
    }
    
    private void UpdateUpgradeUI(Tower tower)
    {
        if (upgradeOptionsContainer != null)
        {
            var upgradeOptions = upgradeOptionsContainer.GetComponentsInChildren<UpgradeMenuOption>();
            foreach (var option in upgradeOptions)
            {
                option.SetTower(tower);
            }
        }
    }
    
    public Tower GetSelectedTower()
    {
        return selectedTower;
    }
}