using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FirewallPowerUp : MonoBehaviour
{
    [Header("Firewall Settings")]
    [SerializeField] private float firewallDuration = 8f;
    [SerializeField] private Vector3 firewallSize = new Vector3(4f, 2f, 0.5f); // Ancho, Alto, Grosor
    [SerializeField] private float firewallHealth = 100f; // Vida de la barrera (opcional)
    [SerializeField] private bool isDestructible = false; // Si los enemigos pueden destruirla
    
    [Header("Placement")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool useMousePlacement = true;
    [SerializeField] private Vector3 fixedPosition = Vector3.zero;
    [SerializeField] private bool allowRotation = true; // Permitir rotar la barrera con scroll
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject firewallPrefab;
    [SerializeField] private Color firewallColor = new Color(1f, 0.3f, 0f, 0.6f); // Naranja transparente
    [SerializeField] private Material firewallMaterial;
    
    [Header("Effects")]
    [SerializeField] private bool damageEnemiesOnContact = false;
    [SerializeField] private float contactDamage = 5f;
    [SerializeField] private float damageInterval = 0.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip deploySound;
    [SerializeField] private AudioClip impactSound;
    
    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownTime = 15f;

    private float cooldownTimer = 0f;

    private bool isOnCooldown = false;
    private bool isPlacingWall = false;
    private bool isFirewallActive = false; // ⭐ NUEVA VARIABLE
    private GameObject currentFirewallPreview;
    private float currentRotationY = 0f;
    private List<GameObject> activeFirewalls = new List<GameObject>();
    
    private void Update()
    {
        if (isPlacingWall && useMousePlacement)
        {
            UpdatePreviewPosition();
            
            // Rotar con la rueda del mouse
            if (allowRotation)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0)
                {
                    currentRotationY += scroll * 90f; // Rotar 90 grados por scroll
                    if (currentFirewallPreview != null)
                    {
                        currentFirewallPreview.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);
                    }
                }
            }
            
            // Clic izquierdo para confirmar
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 position = GetMouseWorldPosition();
                if (position != Vector3.zero)
                {
                    DeployFirewall(position, currentRotationY);
                    isPlacingWall = false;
                    if (currentFirewallPreview != null)
                        Destroy(currentFirewallPreview);
                }
            }
            
            // Clic derecho para cancelar
            if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
    }
    
    public void StartFirewallPlacement()
    {
        if (isOnCooldown)
        {
            Debug.Log("El firewall está en cooldown!");
            return;
        }
        
        if (useMousePlacement)
        {
            isPlacingWall = true;
            currentRotationY = 0f;
            CreatePreview();
        }
        else
        {
            DeployFirewall(fixedPosition, 0f);
        }
    }
    
    private void CreatePreview()
    {
        if (firewallPrefab != null)
        {
            currentFirewallPreview = Instantiate(firewallPrefab);
            currentFirewallPreview.transform.localScale = firewallSize;
        }
        else
        {
            currentFirewallPreview = GameObject.CreatePrimitive(PrimitiveType.Cube);
            currentFirewallPreview.transform.localScale = firewallSize;
            
            Renderer renderer = currentFirewallPreview.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color previewColor = firewallColor;
                previewColor.a *= 0.5f;
                
                Material previewMat = new Material(Shader.Find("Standard"));
                SetupTransparentMaterial(previewMat);
                previewMat.color = previewColor;
                renderer.material = previewMat;
            }
            
            // Remover collider del preview
            Collider col = currentFirewallPreview.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }
    }
    
    private void UpdatePreviewPosition()
    {
        if (currentFirewallPreview == null) return;
        
        Vector3 position = GetMouseWorldPosition();
        if (position != Vector3.zero)
        {
            position.y += firewallSize.y / 2; // Centrar verticalmente
            currentFirewallPreview.transform.position = position;
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
        isPlacingWall = false;
        if (currentFirewallPreview != null)
        {
            Destroy(currentFirewallPreview);
        }
        Debug.Log("Colocación de firewall cancelada");
    }
    
    private void DeployFirewall(Vector3 position, float rotationY)
    {
        StartCoroutine(FirewallRoutine(position, rotationY));
    }
    
    private IEnumerator FirewallRoutine(Vector3 position, float rotationY)
    {
        isOnCooldown = true;
        isFirewallActive = true; // ⭐ Marcar firewall como activo
    
        // Sonido de despliegue
        if (deploySound != null)
        {
            AudioSource.PlayClipAtPoint(deploySound, position);
        }
    
        // Crear el firewall
        GameObject firewall = CreateFirewall(position, rotationY);
        activeFirewalls.Add(firewall);
    
        // Componente para manejar colisiones y daño
        FirewallCollisionHandler handler = firewall.AddComponent<FirewallCollisionHandler>();
        handler.Initialize(firewallHealth, isDestructible, damageEnemiesOnContact, contactDamage, damageInterval, impactSound);
    
        // Animación de aparición
        yield return StartCoroutine(AnimateFirewallAppear(firewall));
    
        // Esperar duración
        float elapsedTime = 0f;
        while (elapsedTime < firewallDuration && firewall != null)
        {
            elapsedTime += Time.deltaTime;
        
            // Parpadeo cuando está por terminar
            if (firewallDuration - elapsedTime < 2f)
            {
                Renderer renderer = firewall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    float alpha = Mathf.PingPong(Time.time * 3f, 0.5f) + 0.3f;
                    Color color = firewallColor;
                    color.a = alpha;
                    renderer.material.color = color;
                }
            }
        
            yield return null;
        }
    
        // Animación de desaparición y destruir
        if (firewall != null)
        {
            yield return StartCoroutine(AnimateFirewallDisappear(firewall));
            activeFirewalls.Remove(firewall);
            Destroy(firewall);
        }
    
        isFirewallActive = false; // ⭐ Marcar firewall como inactivo
        Debug.Log("Firewall desactivado. AHORA iniciando cooldown...");
    
        // Iniciar el cooldown
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
        Debug.Log("Firewall listo para usar de nuevo!");
    }

    public float GetCooldownTimer()
    {
        if (isFirewallActive)
        {
            // Mientras el firewall está activo, mostrar el cooldown completo
            return cooldownTime;
        }
        return cooldownTimer;
    }

public float GetCooldownProgress()
{
    return 1f - (cooldownTimer / cooldownTime);
}

    private GameObject CreateFirewall(Vector3 position, float rotationY)
    {
        GameObject wall;
        position.y += firewallSize.y / 2; // Centrar verticalmente
        
        if (firewallPrefab != null)
        {
            wall = Instantiate(firewallPrefab, position, Quaternion.Euler(0, rotationY, 0));
            wall.transform.localScale = firewallSize;
        }
        else
        {
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = position;
            wall.transform.rotation = Quaternion.Euler(0, rotationY, 0);
            wall.transform.localScale = firewallSize;
            
            // Configurar material
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (firewallMaterial != null)
                {
                    renderer.material = firewallMaterial;
                }
                else
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    SetupTransparentMaterial(mat);
                    mat.color = firewallColor;
                    renderer.material = mat;
                }
            }
        }
        
        // Asegurar que tenga collider para bloquear enemigos
        BoxCollider collider = wall.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = wall.AddComponent<BoxCollider>();
        }
        collider.isTrigger = false; // Debe ser sólido para bloquear
        
        // Hacer el collider un poco más grande para mejor detección
        collider.size = new Vector3(1.1f, 1.1f, 1.1f);
        
        // Configurar el Rigidbody del firewall para que sea inmovible
        Rigidbody wallRb = wall.GetComponent<Rigidbody>();
        if (wallRb == null)
        {
            wallRb = wall.AddComponent<Rigidbody>();
        }
        wallRb.isKinematic = true; // El muro no se mueve
        wallRb.useGravity = false;
        
        wall.name = "Firewall";
        wall.tag = "Firewall"; // Asegurar que tenga un tag
        wall.layer = LayerMask.NameToLayer("Default"); // Asegurar que esté en Default layer
        
        return wall;
    }
    
    private void SetupTransparentMaterial(Material mat)
    {
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
    
    private IEnumerator AnimateFirewallAppear(GameObject firewall)
    {
        Vector3 targetScale = firewall.transform.localScale;
        Vector3 startScale = new Vector3(targetScale.x, 0f, targetScale.z);
        firewall.transform.localScale = startScale;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            firewall.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        firewall.transform.localScale = targetScale;
    }
    
    private IEnumerator AnimateFirewallDisappear(GameObject firewall)
    {
        Vector3 startScale = firewall.transform.localScale;
        Vector3 targetScale = new Vector3(startScale.x, 0f, startScale.z);
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            firewall.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
    }
    
    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }
    
    public bool IsPlacingWall()
    {
        return isPlacingWall;
    }
    
    public void DestroyAllFirewalls()
    {
        foreach (GameObject wall in activeFirewalls)
        {
            if (wall != null)
                Destroy(wall);
        }
        activeFirewalls.Clear();
    }
}

// Clase auxiliar para manejar colisiones del firewall
public class FirewallCollisionHandler : MonoBehaviour
{
    private float health;
    private float maxHealth;
    private bool isDestructible;
    private bool damageOnContact;
    private float contactDamage;
    private float damageInterval;
    private AudioClip impactSound;
    
    private Dictionary<Enemy, float> lastDamageTime = new Dictionary<Enemy, float>();
    
    public void Initialize(float hp, bool destructible, bool doDamage, float damage, float interval, AudioClip sound)
    {
        health = hp;
        maxHealth = hp;
        isDestructible = destructible;
        damageOnContact = doDamage;
        contactDamage = damage;
        damageInterval = interval;
        impactSound = sound;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Sonido de impacto
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, transform.position, 0.5f);
            }
            
            // Detener al enemigo físicamente
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                // Empujar al enemigo hacia atrás
                Vector3 pushDirection = (enemy.transform.position - transform.position).normalized;
                pushDirection.y = 0; // Solo empujar horizontalmente
                rb.AddForce(pushDirection * 5f, ForceMode.Impulse);
            }
            
            // Desactivar el movimiento del enemigo temporalmente
            FollowPathAgent agent = enemy.GetComponent<FollowPathAgent>();
            if (agent != null)
            {
                agent.enabled = false;
                StartCoroutine(ReenableAgent(agent, 0.5f));
            }
            
            // Aplicar daño al firewall si es destructible
            if (isDestructible)
            {
                health -= enemy.DamageToCore;
                Debug.Log($"Firewall recibió {enemy.DamageToCore} de daño. Vida restante: {health}");
                
                if (health <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    
    private void OnCollisionStay(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Mantener al enemigo detenido
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
            
            // Aplicar daño continuo al enemigo si está habilitado
            if (damageOnContact)
            {
                if (!lastDamageTime.ContainsKey(enemy))
                {
                    lastDamageTime[enemy] = 0f;
                }
                
                if (Time.time - lastDamageTime[enemy] >= damageInterval)
                {
                    Health health = enemy.GetComponent<Health>();
                    if (health != null)
                    {
                        health.TakeDamage(contactDamage);
                        lastDamageTime[enemy] = Time.time;
                        Debug.Log($"Firewall dañó al enemigo: {contactDamage}");
                    }
                }
            }
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null && lastDamageTime.ContainsKey(enemy))
        {
            lastDamageTime.Remove(enemy);
        }
    }
    
    private IEnumerator ReenableAgent(FollowPathAgent agent, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (agent != null)
        {
            agent.enabled = true;
        }
    }
    
    
}