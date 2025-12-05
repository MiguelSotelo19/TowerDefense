using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Botón de mejora en el menú radial.
/// Se configura automáticamente según la torre seleccionada.
/// </summary>
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
        {
            button.onClick.AddListener(OnClick);
        }
    }

    public void SetTower(Tower tower)
    {
        currentTower = tower;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentTower == null) return;

        switch (upgradeType)
        {
            case UpgradeType.Upgrade:
                UpdateUpgradeButton();
                break;

            case UpgradeType.Sell:
                UpdateSellButton();
                break;
        }
    }

    private void UpdateUpgradeButton()
    {
        bool canUpgrade = currentTower.CanUpgrade();

        // Habilitar/deshabilitar botón
        if (button != null)
        {
            button.interactable = canUpgrade;
        }

        // Actualizar texto de costo
        if (costText != null)
        {
            if (canUpgrade)
            {
                int cost = currentTower.GetUpgradeCost();
                costText.text = $"${cost}";

                // Verificar si tiene suficiente dinero
                if (EconomyManager.Instance != null)
                {
                    bool canAfford = EconomyManager.Instance.GetBytes() >= cost;
                    costText.color = canAfford ? Color.white : Color.red;
                }
            }
            else
            {
                costText.text = "MAX";
                costText.color = Color.yellow;
            }
        }
    }

    private void UpdateSellButton()
    {
        // Calcular valor de venta (50% del costo total invertido)
        int sellValue = CalculateSellValue();

        if (costText != null)
        {
            costText.text = $"+${sellValue}";
            costText.color = Color.green;
        }

        if (button != null)
        {
            button.interactable = true;
        }
    }

    private void OnClick()
    {
        if (currentTower == null) return;

        switch (upgradeType)
        {
            case UpgradeType.Upgrade:
                TryUpgrade();
                break;

            case UpgradeType.Sell:
                TrySell();
                break;
        }
    }

    private void TryUpgrade()
    {
        if (!currentTower.CanUpgrade())
        {
            Debug.LogWarning("Torre ya está al nivel máximo");
            return;
        }

        int cost = currentTower.GetUpgradeCost();

        // Verificar dinero
        if (EconomyManager.Instance != null)
        {
            if (EconomyManager.Instance.GetBytes() < cost)
            {
                Debug.LogWarning("No tienes suficientes bytes para mejorar");
                return;
            }

            // Pagar y mejorar
            if (EconomyManager.Instance.SpendBytes(cost))
            {
                currentTower.Upgrade();
                Debug.Log($"✅ Torre mejorada a nivel {currentTower.currentLevel}");

                // Actualizar UI
                UpdateUI();

                // Si llegó al máximo, cerrar menú
                if (!currentTower.CanUpgrade())
                {
                    WheelMenuController.Instance.HideMenu();
                }
            }
        }
        else
        {
            // Si no hay EconomyManager, mejorar directamente
            currentTower.Upgrade();
            UpdateUI();
        }
    }

    private void TrySell()
    {
        int sellValue = CalculateSellValue();

        // Devolver dinero
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddBytes(sellValue);
        }

        // Encontrar el spot y eliminar la torre
        if (TowerSpotWD.SelectedSpot != null)
        {
            TowerSpotWD.SelectedSpot.RemoveTower();
            Debug.Log($"💰 Torre vendida por {sellValue} bytes");
        }

        // Cerrar menú
        WheelMenuController.Instance.HideMenu();
    }

    private int CalculateSellValue()
    {
        if (currentTower == null) return 0;

        // Valor = costo base + suma de upgrades hechos (todo al 50%)
        int totalInvested = currentTower.baseCost;

        for (int i = 0; i < currentTower.currentLevel - 1; i++)
        {
            if (i < currentTower.upgradeCosts.Length)
            {
                totalInvested += currentTower.upgradeCosts[i];
            }
        }

        return Mathf.RoundToInt(totalInvested * 0.5f); // 50% de devolución
    }
}

public enum UpgradeType
{
    Upgrade,
    Sell
}