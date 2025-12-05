using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeMenuOption : MonoBehaviour
{
    [Header("Upgrade Type")]
    public UpgradeType upgradeType = UpgradeType.Upgrade;

    [Header("UI References")]
    public TextMeshProUGUI costText;
    public Button button;

    private Tower currentTower;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    public void SetTower(Tower tower)
    {
        currentTower = tower;
        CreateCostTextIfMissing();
        UpdateUI();
    }


    private void UpdateUI()
    {
        if (currentTower == null) return;

        if (upgradeType == UpgradeType.Upgrade)
            UpdateUpgradeButton();
        else
            UpdateSellButton();
    }

    private void UpdateUpgradeButton()
    {
        bool canUpgrade = currentTower.CanUpgrade();

        if (button != null)
            button.interactable = canUpgrade;

        if (costText != null)
        {
            if (canUpgrade)
            {
                int cost = currentTower.GetUpgradeCost();
                string towerName = currentTower.towerName;

                costText.text = $"Mejorar — {cost} bytes";

                if (EconomyManager.Instance != null)
                {
                    bool canAfford = EconomyManager.Instance.GetBytes() >= cost;
                    costText.color = canAfford ? Color.white : Color.red;
                }
            }
            else
            {
                costText.text = $"{currentTower.towerName} (MAX)";
                costText.color = Color.yellow;
            }
        }
    }

    private void UpdateSellButton()
    {
        int sellValue = CalculateSellValue();

        if (costText != null)
        {
            costText.text = $"Vender — +{sellValue} bytes";
            costText.color = Color.white;
        }

        if (button != null)
            button.interactable = true;
    }

    private void OnClick()
    {
        if (currentTower == null) return;

        if (upgradeType == UpgradeType.Upgrade)
            TryUpgrade();
        else
            TrySell();
    }

    private void TryUpgrade()
    {
        if (!currentTower.CanUpgrade())
        {
            Debug.LogWarning("Torre ya está al nivel máximo");
            return;
        }

        int cost = currentTower.GetUpgradeCost();

        if (EconomyManager.Instance != null)
        {
            if (EconomyManager.Instance.GetBytes() < cost)
            {
                Debug.LogWarning("No tienes suficientes bytes para mejorar");
                return;
            }

            if (EconomyManager.Instance.SpendBytes(cost))
            {
                currentTower.Upgrade();
                UpdateUI();

                if (!currentTower.CanUpgrade())
                    WheelMenuController.Instance.HideMenu();
            }
        }
        else
        {
            currentTower.Upgrade();
            UpdateUI();
        }
    }

    private void TrySell()
    {
        int sellValue = CalculateSellValue();

        if (EconomyManager.Instance != null)
            EconomyManager.Instance.AddBytes(sellValue);

        if (TowerSpotWD.SelectedSpot != null)
            TowerSpotWD.SelectedSpot.RemoveTower();

        WheelMenuController.Instance.HideMenu();
    }

    private int CalculateSellValue()
    {
        if (currentTower == null) return 0;

        int total = currentTower.baseCost;

        for (int i = 0; i < currentTower.currentLevel - 1; i++)
        {
            if (i < currentTower.upgradeCosts.Length)
                total += currentTower.upgradeCosts[i];
        }

        return Mathf.RoundToInt(total * 0.5f);
    }
    private void CreateCostTextIfMissing()
    {
        if (costText != null) return;

        GameObject textGO = new GameObject("UpgradeCostText");
        textGO.transform.SetParent(transform, false);

        RectTransform rect = textGO.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        costText = textGO.AddComponent<TextMeshProUGUI>();
        costText.alignment = TextAlignmentOptions.Center;
        costText.enableAutoSizing = true;
        costText.fontSizeMin = 14;
        costText.fontSizeMax = 28;
        costText.color = Color.white;
    }

}

public enum UpgradeType
{
    Upgrade,
    Sell
}
