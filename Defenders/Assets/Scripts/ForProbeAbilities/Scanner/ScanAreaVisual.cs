using UnityEngine;

/// <summary>
/// Script opcional para añadir efectos visuales adicionales al área de escaneo
/// Añade este script al prefab del área de escaneo para efectos extra
/// </summary>
public class ScanAreaVisual : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private bool rotateArea = true;
    [SerializeField] private float rotationSpeed = 30f;
    
    [SerializeField] private bool pulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.1f;
    
    [SerializeField] private bool fadeIn = true;
    [SerializeField] private float fadeInDuration = 0.3f;
    
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem[] particleSystems;
    
    private Vector3 originalScale;
    private Renderer areaRenderer;
    private float elapsedTime = 0f;
    private Color originalColor;
    private float fadeTimer = 0f;
    
    private void Start()
    {
        originalScale = transform.localScale;
        areaRenderer = GetComponent<Renderer>();
        
        if (areaRenderer != null)
        {
            originalColor = areaRenderer.material.color;
            
            if (fadeIn)
            {
                Color transparent = originalColor;
                transparent.a = 0f;
                areaRenderer.material.color = transparent;
            }
        }
        
        // Activar sistemas de partículas
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Play();
            }
        }
    }
    
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        
        // Rotación
        if (rotateArea)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        // Efecto de pulso
        if (pulseEffect)
        {
            float pulse = 1f + Mathf.Sin(elapsedTime * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * pulse;
        }
        
        // Fade in
        if (fadeIn && fadeTimer < fadeInDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, originalColor.a, fadeTimer / fadeInDuration);
            
            if (areaRenderer != null)
            {
                Color currentColor = areaRenderer.material.color;
                currentColor.a = alpha;
                areaRenderer.material.color = currentColor;
            }
        }
    }
    
    private void OnDestroy()
    {
        // Detener partículas
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Stop();
            }
        }
    }
}