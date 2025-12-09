using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScanPowerUp : MonoBehaviour
{
    [Header("Scan Settings")]
    [SerializeField] private float scanRadius = 5f;
    [SerializeField] private float scanDuration = 4f;
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private float damageTickRate = 0.5f;
    
    [Header("Placement")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool useMousePlacement = true;
    [SerializeField] private Vector3 fixedPosition = Vector3.zero;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject scanAreaPrefab;
    [SerializeField] private Color scanColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Material scanMaterial;
    
    [Header("Audio")]
    [SerializeField] private AudioClip scanSound;
    [SerializeField] private AudioClip damageTickSound;
    
    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownTime = 12f;

    private float cooldownTimer = 0f;
    private bool isOnCooldown = false;
    private bool isPlacingArea = false;
    private bool isScanActive = false; // ⭐ NUEVA variable
    private GameObject currentScanAreaPreview;
    
    private void Update()
    {
        if (isPlacingArea && useMousePlacement)
        {
            UpdatePreviewPosition();
            
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
            Debug.Log($"El escaneo está en cooldown! Espera {cooldownTimer:F1} segundos");
            return;
        }
        
        if (useMousePlacement)
        {
            isPlacingArea = true;
            CreatePreview();
        }
        else
        {
            ActivateScanAtPosition(fixedPosition);
        }
    }
    
    private void CreatePreview()
    {
        if (scanAreaPrefab != null)
        {
            currentScanAreaPreview = Instantiate(scanAreaPrefab);
            currentScanAreaPreview.transform.localScale = new Vector3(scanRadius * 2, 0.05f, scanRadius * 2);
            
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
            currentScanAreaPreview = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            currentScanAreaPreview.transform.localScale = new Vector3(scanRadius * 2, 0.05f, scanRadius * 2);
            
            Renderer renderer = currentScanAreaPreview.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color previewColor = scanColor;
                previewColor.a *= 0.5f;
                renderer.material.color = previewColor;
            }
            
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
            position.y += 0.1f;
            currentScanAreaPreview.transform.position = position;
            currentScanAreaPreview.transform.rotation = Quaternion.identity;
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
        isScanActive = true; // ⭐ Marcar scan como activo
        
        // Reproducir sonido de activación
        if (scanSound != null)
        {
            AudioSource.PlayClipAtPoint(scanSound, position);
        }
        
        // Crear el área visual
        GameObject scanArea = CreateScanArea(position);
        
        float elapsedTime = 0f;
        float nextDamageTick = 0f;
        
        List<Enemy> enemiesInArea = new List<Enemy>();
        
        // DURACIÓN DEL SCAN (sin contar cooldown todavía)
        while (elapsedTime < scanDuration)
        {
            elapsedTime += Time.deltaTime;
            nextDamageTick += Time.deltaTime;
            
            // Aplicar daño cada tick
            if (nextDamageTick >= damageTickRate)
            {
                nextDamageTick = 0f;
                ApplyDamageInArea(position, damagePerSecond * damageTickRate);
                
                if (damageTickSound != null)
                {
                    AudioSource.PlayClipAtPoint(damageTickSound, position, 0.3f);
                }
            }
            
            // Efecto visual pulsante
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
        
        isScanActive = false; // ⭐ Marcar scan como inactivo
        Debug.Log("Escaneo completado. AHORA iniciando cooldown...");
        
        // AHORA SÍ iniciar el cooldown DESPUÉS de que el scan desapareció
        cooldownTimer = cooldownTime;
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
        if (isScanActive)
        {
            // Mientras el scan está activo, mostrar el cooldown completo
            return cooldownTime;
        }
        return cooldownTimer;
    }

    public float GetCooldownProgress()
    {
        if (isScanActive)
        {
            return 0f; // Mientras está activo, progress en 0
        }
        return 1f - (cooldownTimer / cooldownTime);
    }
    
    // ⭐ NUEVO método público
    public bool IsScanActive()
    {
        return isScanActive;
    }
    
    private GameObject CreateScanArea(Vector3 position)
    {
        GameObject area;
        position.y += 0.1f;
        
        if (scanAreaPrefab != null)
        {
            area = Instantiate(scanAreaPrefab, position, Quaternion.Euler(0, 0, 0));
            area.transform.localScale = new Vector3(scanRadius * 2, 0.05f, scanRadius * 2);
        }
        else
        {
            area = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            area.transform.position = position;
            area.transform.rotation = Quaternion.identity;
            area.transform.localScale = new Vector3(scanRadius * 2, 0.05f, scanRadius * 2);
            
            Renderer renderer = area.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (scanMaterial != null)
                {
                    renderer.material = scanMaterial;
                }
                else
                {
                    renderer.material = new Material(Shader.Find("Standard"));
                    renderer.material.SetFloat("_Mode", 3);
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
            
            Collider col = area.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }
        
        return area;
    }
    
    private void ApplyDamageInArea(Vector3 center, float damage)
    {
        Collider[] colliders = Physics.OverlapSphere(center, scanRadius);
        int enemiesHit = 0;
        
        foreach (Collider col in colliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
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
    
    private void OnDrawGizmosSelected()
    {
        if (!useMousePlacement)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(fixedPosition, scanRadius);
        }
    }
}