using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class WheelMenuOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tower Settings")]
    public GameObject towerPrefab;
    [Tooltip("Si está en 0, se leerá automáticamente del prefab")]
    public int towerCost = 0;

    [Header("Visual Feedback")]
    public float hoverScale = 1.2f;
    public float scaleSpeed = 10f;
    public Color hoverColor = new Color(1f, 1f, 1f, 1f);
    public Color normalColor = new Color(1f, 1f, 1f, 1f);
    public Color cannotAffordColor = new Color(1f, 0.3f, 0.3f, 0.5f);

    [Header("UI References")]
    public TextMeshProUGUI costText;

    private Vector3 originalScale;
    private Color originalColor;
    private Image image;
    private Vector3 targetScale;
    private Color targetColor;
    private bool canAfford = true;
    private int actualCost; // ← ESTA ES LA VARIABLE QUE FALTABA

    private void Awake()
    {
        // Si no hay costo manual, leer del prefab
        if (towerCost == 0 && towerPrefab != null)
        {
            Tower towerComponent = towerPrefab.GetComponent<Tower>();
            if (towerComponent != null)
            {
                actualCost = towerComponent.baseCost;
                Debug.Log($"✅ Precio leído del prefab '{towerPrefab.name}': {actualCost} bytes");
            }
            else
            {
                Debug.LogError($"❌ El prefab {towerPrefab.name} no tiene componente Tower!");
                actualCost = 100; // Fallback
            }
        }
        else
        {
            // Usar el costo manual
            actualCost = towerCost;
        }
    }

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        image = GetComponent<Image>();
        if (image != null)
        {
            originalColor = image.color;
            normalColor = originalColor;
            targetColor = originalColor;
        }

        // Si NO hay objeto de texto asignado, crearlo automáticamente
        if (costText == null)
        {
            CreateCostTextObject();
        }

        UpdateCostDisplay();
    }


    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);

        if (image != null)
        {
            image.color = Color.Lerp(image.color, targetColor, Time.deltaTime * scaleSpeed);
        }

        // Verificar si se puede pagar
        UpdateAffordability();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
        targetColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        targetColor = canAfford ? normalColor : cannotAffordColor;
    }

    private void UpdateAffordability()
    {
        if (EconomyManager.Instance == null) return;

        bool wasCanAfford = canAfford;
        canAfford = EconomyManager.Instance.CanAfford(actualCost);

        if (wasCanAfford != canAfford)
        {
            targetColor = canAfford ? normalColor : cannotAffordColor;
        }
    }

    private void UpdateCostDisplay()
    {
        if (costText != null)
        {
            if (towerPrefab != null)
            {
                Tower tower = towerPrefab.GetComponent<Tower>();
                string name = tower != null ? tower.towerName : "Torre";

                costText.text = $"{name} — {actualCost} bytes";
            }
            else
            {
                costText.text = $"{actualCost} bytes";
            }

            if (EconomyManager.Instance != null)
            {
                bool affordable = EconomyManager.Instance.CanAfford(actualCost);
                costText.color = affordable ? Color.white : Color.red;
            }
        }
    }


    public void BuildTower()
    {
        if (TowerSpotWD.SelectedSpot == null)
        {
            Debug.LogWarning("⚠️ No hay spot seleccionado!");
            return;
        }

        if (TowerSpotWD.SelectedSpot.IsOccupied())
        {
            Debug.LogWarning("⚠️ Ya hay una torre en este spot!");
            return;
        }

        if (towerPrefab == null)
        {
            Debug.LogError("❌ No hay prefab asignado a este botón!");
            return;
        }

        // Verificar y gastar dinero
        if (EconomyManager.Instance != null)
        {
            if (!EconomyManager.Instance.CanAfford(actualCost))
            {
                Debug.LogWarning($"❌ No tienes suficientes bytes! Necesitas {actualCost}, tienes {EconomyManager.Instance.GetBytes()}");
                return;
            }

            if (!EconomyManager.Instance.SpendBytes(actualCost))
            {
                Debug.LogError("❌ Error al gastar bytes!");
                return;
            }

            Debug.Log($"✅ Gastados {actualCost} bytes. Quedan {EconomyManager.Instance.GetBytes()}");
        }
        else
        {
            Debug.LogWarning("⚠️ No hay EconomyManager! La torre se construirá gratis.");
        }

        // Construir torre
        GameObject towerGO = Instantiate(
            towerPrefab,
            TowerSpotWD.SelectedSpot.transform.position,
            Quaternion.identity
        );

        Tower tower = towerGO.GetComponent<Tower>();
        if (tower != null)
        {
            TowerSpotWD.SelectedSpot.SetTower(tower);
            Debug.Log($"🏗️ Torre '{tower.towerName}' construida en el spot");
        }
        else
        {
            Debug.LogError("❌ El prefab no tiene el componente Tower!");
            Destroy(towerGO);

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddBytes(actualCost);
            }
            return;
        }

        WheelMenuController.Instance.HideMenu();
    }

    private void CreateCostTextObject()
    {
        GameObject textGO = new GameObject("TowerCostText");
        textGO.transform.SetParent(transform, false);

        RectTransform rect = textGO.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; 
        rect.anchorMax = Vector2.one; 
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        costText = textGO.AddComponent<TextMeshProUGUI>();
        costText.fontSize = 20;
        costText.alignment = TextAlignmentOptions.Center;
        costText.color = Color.black;

        // Auto-size opcional
        costText.enableAutoSizing = true;
        costText.fontSizeMin = 12;
        costText.fontSizeMax = 20;
    }

}