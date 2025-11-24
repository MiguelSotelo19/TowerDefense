using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Info")]
    public string towerName = "Antivirus";
    public int currentLevel = 1;
    public int maxLevel = 3;
    
    [Header("Combat Stats")]
    public float range = 10f;
    public string enemyTag = "Enemy";
    public float fireRate = 0.5f; // Tiempo entre disparos
    public int maxTargets = 1; // Cantidad de objetivos simultáneos
    
    [Header("Upgrade Settings")]
    public int baseCost = 100;
    public int[] upgradeCosts = new int[] { 150, 250 }; // Costo nivel 2 y 3
    
    [Header("Visual Settings")]
    public Material[] levelMaterials; // Materiales por nivel
    public GameObject[] levelModels; // Opcional: modelos diferentes
    private MeshRenderer meshRenderer;
    
    [Header("Combat References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject firePoint;
    [SerializeField] private GameObject bulletPool;
    [SerializeField] private GameObject rangeDisplay;
    [SerializeField] private AudioSource shootSound;
    
    private List<Bullet> bullets = new();
    private List<Transform> targets = new List<Transform>(); // Lista de objetivos
    private TowerStateMachine stateMachine;

    public Transform Target
    {
        get { return targets.Count > 0 ? targets[0] : null; }
    }
    
    public List<Transform> Targets => targets;

    private void Awake()
    {
        // Inicializar pool de balas
        for (int i = 0; i < 50; i++)
        {
            var instance = Instantiate(bulletPrefab, bulletPool.transform);
            var bullet = instance.GetComponent<Bullet>();
            bullets.Add(bullet);
            instance.SetActive(false);
        }
        
        stateMachine = new TowerStateMachine(this);
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        
        // Aplicar stats del nivel inicial
        ApplyLevelStats();
    }

    private void Start()
    {
        if (rangeDisplay != null)
        {
            rangeDisplay.transform.localScale = new Vector3(range / 5f, 1f, range / 5f);
            rangeDisplay.SetActive(false);
        }
        
        UpdateVisuals();
    }

    private void Update()
    {
        FindTargets();
        AimAtPrimaryTarget();
        stateMachine.Update();
    }

    public IEnumerator FireRoutine()
    {
        while (targets.Count > 0)
        {
            // Disparar a cada objetivo disponible según maxTargets
            int targetsToFire = Mathf.Min(targets.Count, maxTargets);
            
            for (int i = 0; i < targetsToFire; i++)
            {
                if (targets[i] == null) continue;
                
                var available = bullets.FirstOrDefault(x => !x.gameObject.activeInHierarchy);
                if (available)
                {
                    // Calcular dirección hacia el objetivo específico
                    Vector3 direction = (targets[i].position - firePoint.transform.position).normalized;
                    
                    available.direction = direction;
                    available.transform.position = firePoint.transform.position;
                    available.gameObject.SetActive(true);

                    if (shootSound != null)
                        shootSound.Play();
                }
            }
            
            yield return new WaitForSeconds(fireRate);
        }
    }

    private void FindTargets()
    {
        targets.Clear();
        
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        // Crear lista de enemigos en rango ordenados por distancia
        List<(Enemy enemy, float distance)> enemiesInRange = new List<(Enemy, float)>();
        
        foreach (Enemy enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            
            if (distanceToEnemy <= range)
            {
                enemiesInRange.Add((enemy, distanceToEnemy));
            }
        }
        
        // Ordenar por distancia (más cercano primero)
        enemiesInRange.Sort((a, b) => a.distance.CompareTo(b.distance));
        
        // Tomar los primeros N enemigos según maxTargets
        int count = Mathf.Min(enemiesInRange.Count, maxTargets);
        for (int i = 0; i < count; i++)
        {
            targets.Add(enemiesInRange[i].enemy.transform);
        }
    }

    private void AimAtPrimaryTarget()
    {
        if (targets.Count == 0) return;

        Transform primaryTarget = targets[0];
        if (primaryTarget == null) return;

        Vector3 dir = primaryTarget.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = lookRotation.eulerAngles;

        transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    // ========== SISTEMA DE MEJORAS ==========
    
    public bool CanUpgrade()
    {
        return currentLevel < maxLevel;
    }
    
    public int GetUpgradeCost()
    {
        if (!CanUpgrade()) return 0;
        
        int costIndex = currentLevel - 1; // nivel 1 → índice 0
        return upgradeCosts[costIndex];
    }
    
    public void Upgrade()
    {
        if (!CanUpgrade())
        {
            Debug.LogWarning($"{towerName} ya está al nivel máximo!");
            return;
        }
        
        /*int cost = GetUpgradeCost();
        
        // Verificar dinero
        if (EconomyManager.Instance != null && EconomyManager.Instance.GetBytes() < cost)
        {
            Debug.LogWarning("No hay suficientes bytes para mejorar!");
            return;
        }
        
        // Pagar mejora
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.SpendBytes(cost);
        }*/
        
        // Subir nivel
        currentLevel++;
        
        // Aplicar stats del nuevo nivel
        ApplyLevelStats();
        
        // Actualizar visuales
        UpdateVisuals();
        
        Debug.Log($"{towerName} mejorada a nivel {currentLevel}!");
        Debug.Log(GetTowerInfo());
    }
    
    private void ApplyLevelStats()
    {
        switch (currentLevel)
        {
            case 1: // Antivirus estándar
                fireRate = 0.5f;
                maxTargets = 1;
                break;
                
            case 2: // Antivirus Pro - más rápido
                fireRate = 0.25f; // 2x más rápido
                maxTargets = 1;
                break;
                
            case 3: // Antivirus 360° - dos objetivos
                fireRate = 0.35f; // velocidad intermedia
                maxTargets = 2;
                break;
        }
    }
    
    private void UpdateVisuals()
    {
        // Cambiar material según nivel
        if (levelMaterials != null && levelMaterials.Length > 0 && meshRenderer != null)
        {
            int materialIndex = Mathf.Clamp(currentLevel - 1, 0, levelMaterials.Length - 1);
            if (levelMaterials[materialIndex] != null)
            {
                meshRenderer.material = levelMaterials[materialIndex];
            }
        }
        
        // Cambiar modelo completo si se configuró (opcional)
        if (levelModels != null && levelModels.Length > 0)
        {
            // Desactivar todos
            foreach (var model in levelModels)
            {
                if (model != null) model.SetActive(false);
            }
            
            // Activar el del nivel actual
            int modelIndex = Mathf.Clamp(currentLevel - 1, 0, levelModels.Length - 1);
            if (levelModels[modelIndex] != null)
            {
                levelModels[modelIndex].SetActive(true);
            }
        }
        
        // Actualizar rango visual si existe
        if (rangeDisplay != null)
        {
            rangeDisplay.transform.localScale = new Vector3(range / 5f, 1f, range / 5f);
        }
    }
    
    public string GetTowerInfo()
    {
        string levelName = currentLevel switch
        {
            1 => "Antivirus Estándar",
            2 => "Antivirus Pro",
            3 => "Antivirus 360°",
            _ => "Desconocido"
        };
        
        string info = $"{levelName}\n";
        info += $"Nivel {currentLevel}/{maxLevel}\n";
        info += $"Velocidad: {(1f/fireRate):F1} disparos/s\n";
        info += $"Objetivos: {maxTargets}\n";
        info += $"Rango: {range:F1}\n";
        
        if (CanUpgrade())
        {
            info += $"\nMejora: {GetUpgradeCost()} bytes";
        }
        else
        {
            info += "\n¡NIVEL MÁXIMO!";
        }
        
        return info;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    private void OnMouseEnter()
    {
        if (rangeDisplay != null)
            rangeDisplay.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (rangeDisplay != null)
            rangeDisplay.SetActive(false);
    }
}