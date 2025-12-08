using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FreezeButton : MonoBehaviour
{
    [SerializeField] private FreezePowerUp freezePowerUp;
    [SerializeField] private Button freezeButton;
    
    [Header("Visual Feedback")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Image cooldownOverlay; // Imagen de overlay para el cooldown
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.gray;
    
    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI cooldownText; // Texto para mostrar el tiempo restante
    
    private void Start()
    {
        // Si no se asigna el FreezePowerUp desde el Inspector, buscarlo
        if (freezePowerUp == null)
        {
            freezePowerUp = FindFirstObjectByType<FreezePowerUp>();
            
            if (freezePowerUp == null)
            {
                Debug.LogError("No se encontró FreezePowerUp en la escena. Por favor, añade el script FreezePowerUp a un GameObject.");
            }
        }
        
        // Configurar el botón
        if (freezeButton == null)
            freezeButton = GetComponent<Button>();
            
        if (freezeButton != null)
        {
            freezeButton.onClick.AddListener(OnFreezeButtonClicked);
        }
        
        // Ocultar el texto de cooldown al inicio
        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (freezePowerUp == null) return;
        
        // Actualizar el estado visual del botón
        if (freezePowerUp.IsOnCooldown())
        {
            // En cooldown
            if (buttonImage != null)
                buttonImage.color = cooldownColor;
                
            // Actualizar overlay de cooldown (efecto radial)
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 1f - freezePowerUp.GetCooldownProgress();
            }
            
            // Mostrar tiempo restante
            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = Mathf.Ceil(freezePowerUp.GetCooldownTimer()).ToString();
            }
            
            // Desactivar el botón
            if (freezeButton != null)
                freezeButton.interactable = false;
        }
        else
        {
            // Disponible
            if (buttonImage != null)
                buttonImage.color = normalColor;
                
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = 0f;
                
            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);
                
            if (freezeButton != null)
                freezeButton.interactable = true;
        }
    }
    
    private void OnFreezeButtonClicked()
    {
        if (freezePowerUp != null)
        {
            freezePowerUp.ActivateFreezeAll();
        }
        else
        {
            Debug.LogError("FreezePowerUp no está asignado!");
        }
    }
}