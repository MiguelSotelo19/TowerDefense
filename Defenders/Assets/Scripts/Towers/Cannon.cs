using System.Linq;
using UnityEngine;

/// <summary>
/// Torre Cañón - Dispara más lento pero hace más daño.
/// Nivel 3 hace daño en área (si configuras las balas para explotar).
/// </summary>
public class Cannon : Tower
{
    [Header("Cannon Specific")]
    public float explosionRadius = 0f; // Solo para nivel 3

    protected override void Awake()
    {
        // Configurar solo valores que NO quieres cambiar en el Inspector
        towerName = "Cañón de datos";
        maxLevel = 3;

        // Costos
        baseCost = 150;
        upgradeCosts = new int[] { 200, 350 };

        // Los stats (range, fireRate, etc.) ahora se configuran en el Inspector
        // o los deja con los valores por defecto de Tower.cs

        // Llamar al Awake de Tower
        base.Awake();
    }

    protected override void ApplyLevelStats()
    {
        // Aquí defines qué cambia en cada nivel
        switch (currentLevel)
        {
            case 1: // Cañón de datos (básico)
                fireRate = 1.5f;
                maxTargets = 1;
                damage = 50;
                explosionRadius = 0f;
                break;

            case 2: // Cañón Binario (ataca a 2, menos daño c/u)
                fireRate = 1.2f;
                maxTargets = 2; // Puede atacar a 2 enemigos
                damage = 40; // 80% del daño original (50 * 0.8)
                explosionRadius = 0f;
                break;

            case 3: // Cañón Comprimido (explota en área)
                fireRate = 1.0f;
                maxTargets = 1;
                damage = 60;
                explosionRadius = 4f; // Radio de explosión
                break;
        }
    }

    protected override void FireBullet(Transform target)
    {
        Bullet available = bullets.FirstOrDefault(x => !x.gameObject.activeInHierarchy);
        if (available == null) return;

        Vector3 direction = (target.position - firePoint.transform.position).normalized;

        available.direction = direction;
        available.transform.position = firePoint.transform.position;
        available.SetDamage(damage);
        available.SetExplosionRadius(explosionRadius);
        available.SetFollowTarget(false, null); // o true si quieres que persiga
        available.gameObject.SetActive(true);


        if (shootSound != null)
            shootSound.Play();
    }


    public override string GetTowerInfo()
    {
        string levelName = currentLevel switch
        {
            1 => "Cañón de datos",
            2 => "Cañón Binario",
            3 => "Cañón Comprimido",
            _ => towerName
        };

        string info = $"{levelName}\n";
        info += $"Nivel {currentLevel}/{maxLevel}\n";
        info += $"Velocidad: {(1f / fireRate):F1} disparos/s\n";
        info += $"Daño: {damage}\n";
        info += $"Objetivos: {maxTargets}\n";
        info += $"Rango: {range:F1}\n";

        if (explosionRadius > 0)
        {
            info += $"Radio explosión: {explosionRadius:F1}m\n";
        }

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

    protected override void OnDrawGizmosSelected()
    {
        // Dibujar rango normal
        base.OnDrawGizmosSelected();

        // Si tiene explosión y hay un objetivo, mostrar el radio
        if (explosionRadius > 0 && PrimaryTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(PrimaryTarget.position, explosionRadius);
        }
    }
}