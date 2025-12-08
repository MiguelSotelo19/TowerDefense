using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScanButton : MonoBehaviour
{
    [SerializeField] private ScanPowerUp scanPowerUp;
    [SerializeField] private Button scanButton;
    
    [Header("Visual Feedback")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Image cooldownOverlay; // Imagen de overlay para el cooldown
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.gray;
    [SerializeField] private Color placingColor = new Color(1f, 1f, 0f, 1f); // Amarillo cuando está colocando
    
    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI cooldownText; // Texto para mostrar el tiempo restante
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private string normalText = "Escaneo Rápido";
    [SerializeField] private string placingText = "Clic para colocar (Derecho=Cancelar)";
    
    private void Start()
    {
        if (scanPowerUp == null)
        {
            scanPowerUp = FindFirstObjectByType<ScanPowerUp>();
            
            if (scanPowerUp == null)
            {
                Debug.LogError("No se encontró ScanPowerUp en la escena. Añade el script a un GameObject.");
            }
        }
        
        if (scanButton == null)
            scanButton = GetComponent<Button>();
            
        if (scanButton != null)
        {
            scanButton.onClick.AddListener(OnScanButtonClicked);
        }
        
        // Ocultar el texto de cooldown al inicio
        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (scanPowerUp == null) return;
        
        // Modo colocación (prioridad alta)
        if (scanPowerUp.IsPlacingArea())
        {
            if (buttonImage != null)
                buttonImage.color = placingColor;
                
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = 0f;
                
            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);
                
            if (instructionText != null)
                instructionText.text = placingText;
                
            if (scanButton != null)
                scanButton.interactable = false;
        }
        // Modo cooldown
        else if (scanPowerUp.IsOnCooldown())
        {
            if (buttonImage != null)
                buttonImage.color = cooldownColor;
                
            // Actualizar overlay de cooldown (efecto radial)
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 1f - scanPowerUp.GetCooldownProgress();
            }
            
            // Mostrar tiempo restante
            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = Mathf.Ceil(scanPowerUp.GetCooldownTimer()).ToString();
            }
            
            if (instructionText != null)
                instructionText.gameObject.SetActive(false);
                
            if (scanButton != null)
                scanButton.interactable = false;
        }
        // Modo disponible
        else
        {
            if (buttonImage != null)
                buttonImage.color = normalColor;
                
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = 0f;
                
            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);
                
            if (instructionText != null)
            {
                instructionText.gameObject.SetActive(true);
                instructionText.text = normalText;
            }
                
            if (scanButton != null)
                scanButton.interactable = true;
        }
    }
    
    private void OnScanButtonClicked()
    {
        if (scanPowerUp != null)
        {
            scanPowerUp.StartScanPlacement();
        }
        else
        {
            Debug.LogError("ScanPowerUp no está asignado!");
        }
    }
}