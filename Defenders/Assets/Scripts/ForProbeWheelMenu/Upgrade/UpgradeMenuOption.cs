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
    private bool isProcessing = false; 

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
        isProcessing = false;
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
            button.interactable = canUpgrade && !isProcessing;

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
            button.interactable = !isProcessing; //Esta cosa es para que no hagan +1 accion por vez
    }

    private void OnClick()
    {
        if (isProcessing)
        {
            return;
        }

        if (currentTower == null) return;

        if (upgradeType == UpgradeType.Upgrade)
            TryUpgrade();
        else
            TrySell();
    }

    private void TryUpgrade()
    {
        isProcessing = true;

        if (!currentTower.CanUpgrade())
        {
            isProcessing = false;
            return;
        }

        int cost = currentTower.GetUpgradeCost();

        if (EconomyManager.Instance != null)
        {
            if (EconomyManager.Instance.GetBytes() < cost)
            {
                isProcessing = false;
                return;
            }

            if (EconomyManager.Instance.SpendBytes(cost))
            {
                currentTower.Upgrade();
                WheelMenuController.Instance.HideMenu();
            }
            else
            {
                Debug.LogError("Error al gastar bytes");
                isProcessing = false;
            }
        }
        else
        {
            // Sin EconomyManager (modo debug)
            currentTower.Upgrade();
            WheelMenuController.Instance.HideMenu();
        }
    }

    private void TrySell()
    {
        isProcessing = true;

        int sellValue = CalculateSellValue();

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddBytes(sellValue);
        }

        if (TowerSpotWD.SelectedSpot != null)
        {
            TowerSpotWD.SelectedSpot.RemoveTower();
        }
        else
        {
            Debug.LogError("No hay SelectedSpot! La torre no se puede eliminar correctamente");

            // Fallback: destruir la torre manualmente si no hay spot
            if (currentTower != null)
            {
                Destroy(currentTower.gameObject);
            }
        }

        // Cerrar menú
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