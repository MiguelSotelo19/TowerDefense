using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Info")]
    public string towerName = "Torre";
    public int currentLevel = 1;
    public int maxLevel = 3;

    [Header("Combat Stats")]
    public float range = 5f;
    public float fireRate = 0.5f;
    public int maxTargets = 1;
    public int damage = 25;

    [Header("Economy")]
    public int baseCost = 100;
    public int[] upgradeCosts = new int[] { 150, 250 };

    [Header("References")]
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected GameObject firePoint;
    [SerializeField] protected GameObject bulletPool;
    [SerializeField] protected GameObject rangeDisplay;
    [SerializeField] protected AudioSource shootSound;

    [Header("Visuals")]
    [SerializeField] protected Material[] levelMaterials;
    protected MeshRenderer meshRenderer;


    protected List<Bullet> bullets = new List<Bullet>();
    protected List<Transform> targets = new List<Transform>();
    protected TowerStateMachine stateMachine;

    public Transform PrimaryTarget => targets.Count > 0 ? targets[0] : null;
    public List<Transform> Targets => targets; 


    protected virtual void Awake()
    {
        InitializeBullets();
        stateMachine = new TowerStateMachine(this);
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    protected virtual void Start()
    {
        if (rangeDisplay != null)
        {
            float scale = (range * 2f) / 10f;
            rangeDisplay.transform.localScale = new Vector3(scale, 1f, scale);


            rangeDisplay.SetActive(false);
        }

        UpdateVisuals();
    }

    protected virtual void Update()
    {
        FindTargets();
        AimAtPrimaryTarget();
        stateMachine?.Update();
    }

    private void InitializeBullets()
    {
        if (bulletPrefab == null || bulletPool == null) return;

        for (int i = 0; i < 50; i++)
        {
            GameObject instance = Instantiate(bulletPrefab, bulletPool.transform);
            Bullet bullet = instance.GetComponent<Bullet>();
            bullets.Add(bullet);
            instance.SetActive(false);
        }
    }

    public virtual IEnumerator FireRoutine()
    {
        while (targets.Count > 0)
        {
            FireAtTargets();
            yield return new WaitForSeconds(fireRate);
        }
    }

    protected virtual void FireAtTargets()
    {
        int targetsToFire = Mathf.Min(targets.Count, maxTargets);

        for (int i = 0; i < targetsToFire; i++)
        {
            if (targets[i] == null) continue;
            FireBullet(targets[i]);
        }
    }

    protected virtual void FireBullet(Transform target)
    {
        Bullet available = bullets.FirstOrDefault(x => !x.gameObject.activeInHierarchy);
        if (available == null) return;

        Vector3 direction = (target.position - firePoint.transform.position).normalized;

        available.direction = direction;
        available.transform.position = firePoint.transform.position;

        available.SetDamage(damage);
        available.SetSpeed(available.speed); 
        available.SetMaxRange(available.maxRange);
        available.SetPierce(false); 
        available.SetExplosionRadius(0f);

        available.SetFollowTarget(false, null);

        available.gameObject.SetActive(true);

        if (shootSound != null)
            shootSound.Play();
    }

    protected virtual void FindTargets()
    {
        targets.Clear();

        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        List<(Enemy enemy, float distance)> enemiesInRange = new List<(Enemy, float)>();

        foreach (Enemy enemy in enemies)
        {
            float dist = Vector3.Distance(meshRenderer.bounds.center, enemy.transform.position);

            if (dist <= range)
            {
                enemiesInRange.Add((enemy, dist));
            }
        }

        enemiesInRange.Sort((a, b) => a.distance.CompareTo(b.distance));

        int count = Mathf.Min(enemiesInRange.Count, maxTargets);
        for (int i = 0; i < count; i++)
        {
            targets.Add(enemiesInRange[i].enemy.transform);
        }
    }

    protected virtual void AimAtPrimaryTarget()
    {
        if (PrimaryTarget == null) return;

        Vector3 dir = PrimaryTarget.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = lookRotation.eulerAngles;

        transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    public bool CanUpgrade()
    {
        return currentLevel < maxLevel;
    }

    public int GetUpgradeCost()
    {
        if (!CanUpgrade()) return 0;

        int costIndex = currentLevel - 1;
        if (costIndex >= upgradeCosts.Length) return 0;

        return upgradeCosts[costIndex];
    }

    public virtual void Upgrade()
    {
        if (!CanUpgrade())
        {
            Debug.LogWarning($"{towerName} ya está al nivel máximo!");
            return;
        }

        currentLevel++;
        ApplyLevelStats();
        UpdateVisuals();

        Debug.Log($"{towerName} mejorada a nivel {currentLevel}!");
    }

    protected virtual void ApplyLevelStats()
    {
        // Las clases hijas definen qué pasa en cada nivel
    }

    protected virtual void UpdateVisuals()
    {
        // Cambiar material
        if (levelMaterials != null && levelMaterials.Length > 0 && meshRenderer != null)
        {
            int index = Mathf.Clamp(currentLevel - 1, 0, levelMaterials.Length - 1);
            if (levelMaterials[index] != null)
            {
                meshRenderer.material = levelMaterials[index];
            }
        }

        // Actualizar rango visual
        if (rangeDisplay != null)
        {
            rangeDisplay.transform.localScale = new Vector3(range / 5f, 1f, range / 5f);
        }
    }

    public virtual string GetTowerInfo()
    {
        string info = $"{towerName}\n";
        info += $"Nivel {currentLevel}/{maxLevel}\n";
        info += $"Velocidad: {(1f / fireRate):F1} disparos/s\n";
        info += $"Daño: {damage}\n";
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

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    protected virtual void OnMouseEnter()
    {
        if (rangeDisplay != null)
            rangeDisplay.SetActive(true);
    }

    protected virtual void OnMouseExit()
    {
        if (rangeDisplay != null)
            rangeDisplay.SetActive(false);
    }

    protected virtual void OnMouseDown()
    {
        // Ignorar clic
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;
        
        // Buscar spot torre
        TowerSpotWD parentSpot = GetComponentInParent<TowerSpotWD>();
        if (parentSpot == null)
        {
            TowerSpotWD[] allSpots = FindObjectsByType<TowerSpotWD>(FindObjectsSortMode.None);
            float closestDistance = float.MaxValue;
            
            foreach (var spot in allSpots)
            {
                float distance = Vector3.Distance(transform.position, spot.transform.position);
                if (distance < closestDistance && distance < 0.5f) // Muy cerca = es su spot
                {
                    closestDistance = distance;
                    parentSpot = spot;
                }
            }
        }
        
        if (parentSpot != null)
        {
            TowerSpotWD.SelectedSpot = parentSpot;
            
            Vector3 mousePos = Input.mousePosition;
            WheelMenuController.Instance.ShowUpgradeMenu(mousePos, this);
        }
        else
        {
            Debug.LogWarning("No se encontró TowerSpot");
        }
    }
}