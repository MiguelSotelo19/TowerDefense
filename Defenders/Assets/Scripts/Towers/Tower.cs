using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Clase base simple para todas las torres.
/// Tiene la funcionalidad común: disparar, detectar enemigos, mejorar.
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("Tower Info")]
    public string towerName = "Torre";
    public int currentLevel = 1;
    public int maxLevel = 3;

    [Header("Combat Stats")]
    public float range = 5f; // ← Cambiado a 5 como querías
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

    // Sistema interno
    protected List<Bullet> bullets = new List<Bullet>();
    protected List<Transform> targets = new List<Transform>();
    protected TowerStateMachine stateMachine;

    // Propiedades públicas para acceder a los targets
    public Transform PrimaryTarget => targets.Count > 0 ? targets[0] : null;
    public List<Transform> Targets => targets; // ← ESTO FALTABA

    // ========== UNITY LIFECYCLE ==========

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

    // ========== INICIALIZACIÓN ==========

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

    // ========== SISTEMA DE DISPARO ==========

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
        // Dispara a los primeros N enemigos según maxTargets
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

        // Asignar propiedades a la bala desde la torre (importante)
        available.direction = direction;
        available.transform.position = firePoint.transform.position;

        // Pasar stats que dependen de la torre (estimulación para que el prefab no domine)
        available.SetDamage(damage);
        available.SetSpeed(available.speed); // opcional: si quieres escalar speed por torre, reemplaza available.speed por un valor calculado
        available.SetMaxRange(available.maxRange); // si quieres cambiar maxRange desde la torre, pásalo aquí
        available.SetPierce(false); // default; las torres que permiten pierce pueden sobrescribir en su FireBullet
        available.SetExplosionRadius(0f); // base: sin explosión; Cannon sobrescribe

        // Si quieres que la bala siga al objetivo por defecto comentalo:
        available.SetFollowTarget(false, null);

        available.gameObject.SetActive(true);

        if (shootSound != null)
            shootSound.Play();
    }


    // ========== DETECTAR ENEMIGOS ==========

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

        // Ordenar por distancia
        enemiesInRange.Sort((a, b) => a.distance.CompareTo(b.distance));

        // Tomar los más cercanos
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

    // ========== SISTEMA DE MEJORAS ==========

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
        ApplyLevelStats(); // Las torres específicas implementan esto
        UpdateVisuals();

        Debug.Log($"{towerName} mejorada a nivel {currentLevel}!");
    }

    // Este método lo sobrescribe cada torre para cambiar sus stats
    protected virtual void ApplyLevelStats()
    {
        // Las clases hijas definen qué pasa en cada nivel
    }

    // ========== VISUALES ==========

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

    // ========== INFORMACIÓN ==========

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

    // ========== GIZMOS & MOUSE ==========

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

    // ========== CLICK HANDLING ==========

    protected virtual void OnMouseDown()
    {
        // Ignorar si hacemos click sobre UI
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        // Buscar el spot en el que está esta torre
        TowerSpotWD parentSpot = GetComponentInParent<TowerSpotWD>();
        if (parentSpot == null)
        {
            // Si la torre no es hija del spot, buscar el más cercano
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
            // Seleccionar el spot
            TowerSpotWD.SelectedSpot = parentSpot;

            // Mostrar menú de upgrade
            Vector3 mousePos = Input.mousePosition;
            WheelMenuController.Instance.ShowUpgradeMenu(mousePos, this);

            Debug.Log($"🔧 Click en torre {towerName} - Mostrando menú de upgrade");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró TowerSpot para esta torre!");
        }
    }
}