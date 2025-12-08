using UnityEngine;
using System.Collections;
using System.Linq;

public class Algoritmo : Tower
{
    [Header("Algoritmo Specific")]
    [SerializeField] private float damageMultiplierPerHit = 1.5f;
    [SerializeField] private float damageIncreasePerSecond = 0.1f; 
    [SerializeField] private int maxEvolutiveDamage = 50;
    [SerializeField] private int editorCost = 80;
    [SerializeField] private int[] editorUpgradeCosts = new int[] { 120, 200 };


    private Transform lastTarget; 
    private int consecutiveHits = 0; 
    private float gameTime = 0f;

    protected override void Awake()
    {
        towerName = "Algoritmo Predictivo";
        maxLevel = 3;

        // Asignar a Tower los valores que Unity sí puede leer en Inspector
        baseCost = editorCost;
        upgradeCosts = editorUpgradeCosts;

        base.Awake();
    }


    protected override void Start()
    {
        base.Start();

        gameTime = 0f;
    }

    protected override void Update()
    {
        base.Update();

        if (currentLevel == 3)
        {
            gameTime += Time.deltaTime;
        }
    }

    protected override void ApplyLevelStats()
    {
        switch (currentLevel)
        {
            case 1:
                range = 6f;
                fireRate = 0.30f;
                maxTargets = 1;
                damage = 5; 
                break;

            case 2: 
                range = 6f;
                fireRate = 0.28f;
                maxTargets = 1;
                damage = 5;
                break;

            case 3:
                range = 6f;
                fireRate = 0.28f;
                maxTargets = 1;
                damage = 5;
                gameTime = 0f;
                break;
        }

        ResetAdaptiveStats();
    }

    protected override void FireAtTargets()
    {
        if (PrimaryTarget == null) return;

        if (currentLevel == 2)
        {
            if (lastTarget == PrimaryTarget)
            {
                // Mismo enemigo: incrementar multiplicador
                consecutiveHits++;
            }
            else
            {
                // Enemigo diferente: resetear
                consecutiveHits = 0;
                lastTarget = PrimaryTarget;
            }
        }

        base.FireAtTargets();
    }

    protected override void FireBullet(Transform target)
    {
        var available = bullets.FirstOrDefault(x => !x.gameObject.activeInHierarchy);
        if (available == null) return;

        Vector3 direction = (target.position - firePoint.transform.position).normalized;

        int finalDamage = CalculateFinalDamage();

        available.direction = direction;
        available.transform.position = firePoint.transform.position;
        available.SetDamage(finalDamage);
        available.SetSpeed(available.speed);
        available.SetMaxRange(available.maxRange);
        available.SetPierce(false);
        available.SetExplosionRadius(0f);
        available.SetFollowTarget(false, null);
        available.gameObject.SetActive(true);

        if (shootSound != null)
            shootSound.Play();
    }

    private int CalculateFinalDamage()
    {
        switch (currentLevel)
        {
            case 1:
                return damage;

            case 2: // Nivel 2: Daño incremental
                // damage * (multiplier ^ consecutiveHits)
                // Ej: 5 * (1.5^0) = 5
                //     5 * (1.5^1) = 7.5 → 8
                //     5 * (1.5^2) = 11.25 → 11
                float adaptiveDamage = damage * Mathf.Pow(damageMultiplierPerHit, consecutiveHits);
                return Mathf.RoundToInt(adaptiveDamage);

            case 3: // Nivel 3: Daño por tiempo
                // damage + (bonus * segundos)
                // Ej con bonus 0.1: 
                //     10 seg: 5 + (0.1 * 10) = 6
                //     60 seg: 5 + (0.1 * 60) = 11
                //     300 seg (5 min): 5 + (0.1 * 300) = 35
                float timeDamage = damage + (damageIncreasePerSecond * gameTime);
                int evolutiveDamage = Mathf.RoundToInt(timeDamage);

                // Limitar daño máximo para no romper el balance
                return Mathf.Min(evolutiveDamage, maxEvolutiveDamage);

            default:
                return damage;
        }
    }

    /// <summary>
    /// Resetea las estadísticas adaptativas
    /// </summary>
    private void ResetAdaptiveStats()
    {
        lastTarget = null;
        consecutiveHits = 0;
    }

    public override string GetTowerInfo()
    {
        string levelName = currentLevel switch
        {
            1 => "Algoritmo Predictivo",
            2 => "Algoritmo Adaptativo",
            3 => "Algoritmo Evolutivo",
            _ => towerName
        };

        string info = $"{levelName}\n";
        info += $"Nivel {currentLevel}/{maxLevel}\n";
        info += $"Velocidad: {(1f / fireRate):F1} disparos/s\n";
        info += $"Rango: {range:F1}\n";

        // Mostrar daño según nivel
        switch (currentLevel)
        {
            case 1:
                info += $"Daño: {damage}\n";
                break;

            case 2:
                info += $"Daño base: {damage}\n";
                info += $"Multiplicador: x{damageMultiplierPerHit} por hit\n";
                if (consecutiveHits > 0)
                {
                    info += $"Daño actual: {CalculateFinalDamage()} ({consecutiveHits} hits)\n";
                }
                break;

            case 3:
                int currentDamage = CalculateFinalDamage();
                int timeMinutes = Mathf.FloorToInt(gameTime / 60f);
                int timeSeconds = Mathf.FloorToInt(gameTime % 60f);

                info += $"Daño base: {damage}\n";
                info += $"+{damageIncreasePerSecond} por segundo\n";
                info += $"Tiempo: {timeMinutes}:{timeSeconds:D2}\n";
                info += $"Daño actual: {currentDamage}\n";
                break;
        }

        info += $"Objetivos: {maxTargets}\n";

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
}