using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FirewallButton : MonoBehaviour
{
    [SerializeField] private FirewallPowerUp firewallPowerUp;
    [SerializeField] private Button firewallButton;
    
    [Header("Visual Feedback")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Image cooldownOverlay; // Imagen de overlay para el cooldown
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.gray;
    [SerializeField] private Color placingColor = new Color(1f, 0.5f, 0f, 1f); // Naranja
    
    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI cooldownText; // Texto para mostrar el tiempo restante
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private string normalText = "Firewall de Emergencia";
    [SerializeField] private string placingText = "Clic=Colocar | Scroll=Rotar | Derecho=Cancelar";
    
    private void Start()
    {
        if (firewallPowerUp == null)
        {
            firewallPowerUp = FindFirstObjectByType<FirewallPowerUp>();
            
            if (firewallPowerUp == null)
            {
                Debug.LogError("No se encontró FirewallPowerUp en la escena.");
            }
        }
        
        if (firewallButton == null)
            firewallButton = GetComponent<Button>();
            
        if (firewallButton != null)
        {
            firewallButton.onClick.AddListener(OnFirewallButtonClicked);
        }
        
        // Ocultar el texto de cooldown al inicio
        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (firewallPowerUp == null) return;
        
        // Modo colocación (prioridad alta)
        if (firewallPowerUp.IsPlacingWall())
        {
            if (buttonImage != null)
                buttonImage.color = placingColor;
                
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = 0f;
                
            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);
                
            if (instructionText != null)
                instructionText.text = placingText;
                
            if (firewallButton != null)
                firewallButton.interactable = false;
        }
        // Modo cooldown
        else if (firewallPowerUp.IsOnCooldown())
        {
            if (buttonImage != null)
                buttonImage.color = cooldownColor;
                
            // Actualizar overlay de cooldown (efecto radial)
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 1f - firewallPowerUp.GetCooldownProgress();
            }
            
            // Mostrar tiempo restante
            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = Mathf.Ceil(firewallPowerUp.GetCooldownTimer()).ToString();
            }
            
            if (instructionText != null)
                instructionText.gameObject.SetActive(false);
                
            if (firewallButton != null)
                firewallButton.interactable = false;
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
                
            if (firewallButton != null)
                firewallButton.interactable = true;
        }
    }
    
    private void OnFirewallButtonClicked()
    {
        if (firewallPowerUp != null)
        {
            firewallPowerUp.StartFirewallPlacement();
        }
        else
        {
            Debug.LogError("FirewallPowerUp no está asignado!");
        }
    }
}