using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeMenuOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Option Type")]
    public OptionType optionType = OptionType.Upgrade;
    
    [Header("Visual Feedback")]
    public float hoverScale = 1.2f;
    public float scaleSpeed = 10f;
    public Color hoverColor = new Color(1f, 1f, 1f, 1f);
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    [Header("UI References")]
    public TextMeshProUGUI costText;
    public Image iconImage;
    
    private Vector3 originalScale;
    private Color originalColor;
    private Image image;
    private Vector3 targetScale;
    private Color targetColor;
    private Tower currentTower;
    private bool isInteractable = true;

    public enum OptionType
    {
        Upgrade,
        Sell,
        Info
    }

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
        if (!isInteractable) return;
        
        targetScale = originalScale * hoverScale;
        targetColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        targetColor = isInteractable ? originalColor : disabledColor;
    }
    
    public void SetTower(Tower tower)
    {
        currentTower = tower;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (currentTower == null) return;
        
        switch (optionType)
        {
            case OptionType.Upgrade:
                UpdateUpgradeUI();
                break;
                
            case OptionType.Sell:
                UpdateSellUI();
                break;
                
            case OptionType.Info:
                UpdateInfoUI();
                break;
        }
    }
    
    private void UpdateUpgradeUI()
    {
        bool canUpgrade = currentTower.CanUpgrade();
        isInteractable = canUpgrade;
        
        if (costText != null)
        {
            if (canUpgrade)
            {
                int cost = currentTower.GetUpgradeCost();
                bool hasEnoughMoney = EconomyManager.Instance != null && 
                                     EconomyManager.Instance.GetBytes() >= cost;
                
                costText.text = $"{cost}";
                costText.color = hasEnoughMoney ? Color.white : Color.red;
                isInteractable = hasEnoughMoney;
            }
            else
            {
                costText.text = "MAX";
                costText.color = Color.yellow;
            }
        }
        
        if (image != null)
        {
            targetColor = isInteractable ? originalColor : disabledColor;
        }
    }
    
    private void UpdateSellUI()
    {
        isInteractable = true;
        
        if (costText != null)
        {
            int sellValue = Mathf.RoundToInt(currentTower.baseCost * 0.5f);
            costText.text = $"+{sellValue}";
            costText.color = Color.green;
        }
    }
    
    private void UpdateInfoUI()
    {
        isInteractable = true;
        
        if (costText != null)
        {
            costText.text = $"Lv.{currentTower.currentLevel}";
        }
    }

    public void OnClick()
    {
        if (!isInteractable || currentTower == null) return;
        
        switch (optionType)
        {
            case OptionType.Upgrade:
                UpgradeTower();
                break;
                
            case OptionType.Sell:
                SellTower();
                break;
                
            case OptionType.Info:
                ShowInfo();
                break;
        }
    }
    
    private void UpgradeTower()
    {
        if (currentTower.CanUpgrade())
        {
            currentTower.Upgrade();
            UpdateUI();
        }
    }
    
    private void SellTower()
    {
        if (TowerSpotWD.SelectedSpot != null)
        {
            int sellValue = Mathf.RoundToInt(currentTower.baseCost * 0.5f);
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddBytes(sellValue);
            }
            
            TowerSpotWD.SelectedSpot.RemoveTower();
            WheelMenuController.Instance.HideMenu();
        }
    }
    
    private void ShowInfo()
    {
        Debug.Log(currentTower.GetTowerInfo());
        
        // Cerrar menú después de mostrar info
        WheelMenuController.Instance.HideMenu();
    }
}