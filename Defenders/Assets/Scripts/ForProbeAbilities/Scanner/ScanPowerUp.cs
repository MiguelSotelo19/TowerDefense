using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScanPowerUp : MonoBehaviour
{
    [Header("Scan Settings")]
    [SerializeField] private float scanRadius = 5f;
    [SerializeField] private float scanDuration = 4f;
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private float damageTickRate = 0.5f; // Cada cuánto tiempo aplica daño
    
    [Header("Placement")]
    [SerializeField] private LayerMask groundLayer; // Para detectar dónde colocar el área
    [SerializeField] private bool useMousePlacement = true; // Si es false, se coloca en el centro del mapa
    [SerializeField] private Vector3 fixedPosition = Vector3.zero; // Posición fija si no usa mouse
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject scanAreaPrefab; // Prefab del área visual
    [SerializeField] private Color scanColor = new Color(0f, 1f, 0f, 0.3f); // Verde transparente
    [SerializeField] private Material scanMaterial; // Material del área
    
    [Header("Audio")]
    [SerializeField] private AudioClip scanSound;
    [SerializeField] private AudioClip damageTickSound;
    
    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownTime = 12f; // Tiempo de recarga en segundos

    private float cooldownTimer = 0f;
    
    private bool isOnCooldown = false;
    private bool isPlacingArea = false;
    private GameObject currentScanAreaPreview;
    
    private void Update()
    {
        // Si estamos en modo de colocación, mostrar preview
        if (isPlacingArea && useMousePlacement)
        {
            UpdatePreviewPosition();
            
            // Clic izquierdo para confirmar posición
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 position = GetMouseWorldPosition();
                if (position != Vector3.zero)
                {
                    ActivateScanAtPosition(position);
                    isPlacingArea = false;
                    if (currentScanAreaPreview != null)
                        Destroy(currentScanAreaPreview);
                }
            }
            
            // Clic derecho para cancelar
            if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
    }
    
    public void StartScanPlacement()
    {
        if (isOnCooldown)
        {
            Debug.Log("El escaneo está en cooldown!");
            return;
        }
        
        if (useMousePlacement)
        {
            // Activar modo de colocación
            isPlacingArea = true;
            CreatePreview();
        }
        else
        {
            // Activar directamente en posición fija
            ActivateScanAtPosition(fixedPosition);
        }
    }
    
    private void CreatePreview()
    {
        if (scanAreaPrefab != null)
        {
            currentScanAreaPreview = Instantiate(scanAreaPrefab);
            // Hacer el preview delgado
            currentScanAreaPreview.transform.localScale = new Vector3(scanRadius * 2, 0.05f, scanRadius * 2);
            
            // Hacer el preview semi-transparente
            Renderer renderer = currentScanAreaPreview.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color previewColor = scanColor;
                previewColor.a *= 0.5f;
                renderer.material.color = previewColor;
            }
        }
        else
        {
            // Crear un preview simple si no hay prefab
            currentScanAreaPreview = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            // Hacer el cilindro muy delgado (Y = 0.05)
            currentScanAreaPreview.transform.localScale = new Vector3(scanRadius * 2, 0.05f, scanRadius * 2);
            
            Renderer renderer = currentScanAreaPreview.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color previewColor = scanColor;
                previewColor.a *= 0.5f;
                renderer.material.color = previewColor;
            }
            
            // Remover el collider del preview
            Collider col = currentScanAreaPreview.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }
    }
    
    private void UpdatePreviewPosition()
    {
        if (currentScanAreaPreview == null) return;
        
        Vector3 position = GetMouseWorldPosition();
        if (position != Vector3.zero)
        {
            position.y += 0.1f; // Elevar ligeramente
            currentScanAreaPreview.transform.position = position;
            currentScanAreaPreview.transform.rotation = Quaternion.identity; // Mantener sin rotación
        }
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            return hit.point;
        }
        
        return Vector3.zero;
    }
    
    private void CancelPlacement()
    {
        isPlacingArea = false;
        if (currentScanAreaPreview != null)
        {
            Destroy(currentScanAreaPreview);
        }
        Debug.Log("Colocación de escaneo cancelada");
    }
    
    private void ActivateScanAtPosition(Vector3 position)
    {
        StartCoroutine(ScanAreaRoutine(position));
    }
    
    private IEnumerator ScanAreaRoutine(Vector3 position)
{
    isOnCooldown = true;
    cooldownTimer = cooldownTime; // Iniciar el timer
    
    // Reproducir sonido de activación
    if (scanSound != null)
    {
        AudioSource.PlayClipAtPoint(scanSound, position);
    }
    
    // Crear el área visual
    GameObject scanArea = CreateScanArea(position);
    
    float elapsedTime = 0f;
    float nextDamageTick = 0f;
    
    // Lista para trackear enemigos que ya están en el área
    List<Enemy> enemiesInArea = new List<Enemy>();
    
    while (elapsedTime < scanDuration)
    {
        elapsedTime += Time.deltaTime;
        nextDamageTick += Time.deltaTime;
        cooldownTimer -= Time.deltaTime; // Reducir el timer mientras dura el scan
        
        // Aplicar daño cada tick
        if (nextDamageTick >= damageTickRate)
        {
            nextDamageTick = 0f;
            ApplyDamageInArea(position, damagePerSecond * damageTickRate);
            
            // Sonido de tick de daño
            if (damageTickSound != null)
            {
                AudioSource.PlayClipAtPoint(damageTickSound, position, 0.3f);
            }
        }
        
        // Efecto visual pulsante opcional (solo en X y Z, no en Y)
        if (scanArea != null)
        {
            float pulse = 1f + Mathf.Sin(elapsedTime * 8f) * 0.05f;
            scanArea.transform.localScale = new Vector3(scanRadius * 2 * pulse, 0.05f, scanRadius * 2 * pulse);
        }
        
        yield return null;
    }
    
    // Destruir el área visual
    if (scanArea != null)
    {
        Destroy(scanArea);
    }
    
    Debug.Log("Escaneo completado. Iniciando cooldown...");
    
    // Continuar el cooldown si aún queda tiempo
    yield return StartCoroutine(CooldownRoutine());
}

private IEnumerator CooldownRoutine()
{
    while (cooldownTimer > 0)
    {
        cooldownTimer -= Time.deltaTime;
        yield return null;
    }
    
    cooldownTimer = 0f;
    isOnCooldown = false;
    Debug.Log("Escaneo listo para usar de nuevo!");
}

public float GetCooldownTimer()
{
    return cooldownTimer;
}

public float GetCooldownProgress()
{
    return 1f - (cooldownTimer / cooldownTime);
}
    
    private GameObject CreateScanArea(Vector3 position)
    {
        GameObject area;
        
        // Ajustar posición para que esté pegada al suelo
        position.y += 0.1f; // Elevar ligeramente para evitar z-fighting con el suelo
        
        if (scanAreaPrefab != null)
        {
            area = Instantiate(scanAreaPrefab, position, Quaternion.Euler(0, 0, 0));
            // Escala: radio en X y Z, pero Y muy pequeño para que sea delgado
            area.transform.localScale = new Vector3(scanRadius * 2, 0.05f, scanRadius * 2);
        }
        else
        {
            // Crear un cilindro simple como área visual
            area = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            area.transform.position = position;
            area.transform.rotation = Quaternion.identity; // Sin rotación
            // Hacer el cilindro muy delgado (Y = 0.05)
            area.transform.localScale = new Vector3(scanRadius * 2, 0.05f, scanRadius * 2);
            
            // Aplicar material y color
            Renderer renderer = area.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (scanMaterial != null)
                {
                    renderer.material = scanMaterial;
                }
                else
                {
                    // Crear material transparente si no existe
                    renderer.material = new Material(Shader.Find("Standard"));
                    renderer.material.SetFloat("_Mode", 3); // Transparent mode
                    renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    renderer.material.SetInt("_ZWrite", 0);
                    renderer.material.DisableKeyword("_ALPHATEST_ON");
                    renderer.material.EnableKeyword("_ALPHABLEND_ON");
                    renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    renderer.material.renderQueue = 3000;
                }
                renderer.material.color = scanColor;
            }
            
            // Remover el collider
            Collider col = area.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }
        
        return area;
    }
    
    private void ApplyDamageInArea(Vector3 center, float damage)
    {
        // Encontrar todos los enemigos en el radio
        Collider[] colliders = Physics.OverlapSphere(center, scanRadius);
        int enemiesHit = 0;
        
        foreach (Collider col in colliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                // Aplicar daño al enemigo
                Health health = enemy.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                    enemiesHit++;
                }
            }
        }
        
        if (enemiesHit > 0)
        {
            Debug.Log($"Escaneo dañó a {enemiesHit} enemigos con {damage} de daño");
        }
    }
    
    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }
    
    public bool IsPlacingArea()
    {
        return isPlacingArea;
    }
    
    // Método para dibujar el radio en el editor
    private void OnDrawGizmosSelected()
    {
        if (!useMousePlacement)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(fixedPosition, scanRadius);
        }
    }
}